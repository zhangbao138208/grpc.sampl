using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebApplication.Protos;

namespace GrpcClient
{
    
    class Program
    {
        private static string _token;
        private static DateTime _expiration = DateTime.MinValue;
        static async Task Main(string[] args)
        {
           
            using var channel = GrpcChannel.ForAddress("https://localhost:5006");

            var client = new EmployeeService.EmployeeServiceClient(channel);

            //var input = int.Parse(args[0]);
            var input = 2;

            switch (input)
            {
                case 1:
                    await GetByNoAsync(client);
                    break;
                case 2:
                    await GetAllAsync(client);
                    break;
                case 3:
                    await AddPhotoAsync(client);
                    break;
                case 4:
                    await Save(client);
                    break;
                case 5:
                    await SaveAll(client);
                    break;
                default:
                    break;
            }

            Console.ReadKey();
        }

        private static bool NeedToken() => string.IsNullOrWhiteSpace(_token) || _expiration > DateTime.UtcNow;

        private static async Task<bool> CreateTokenAsync(EmployeeService.EmployeeServiceClient client)
        {
            var request = new TokenRequest
            {
                Username= "111@qq.com",
                Passward="111@Zb"
            };
            var response = await client.CreateTokenAsync(request);

            if (response.Success)
            {
                _token = response.Token;
                _expiration = response.Expiration.ToDateTime();
                return true;
                
            }
            return false;

        }

        static async Task GetByNoAsync(EmployeeService.EmployeeServiceClient client)
        {
            var md = new Metadata
            {
                {"name","dava" },
                {"role","administrator" }
            };
            var response = await client.GetByNoAsync(new GetByNoRequest
            {
                No = 1993
            }, md);

            Console.WriteLine(response);
        }

        static async Task GetAllAsync(EmployeeService.EmployeeServiceClient client)
        {
            if (!NeedToken()||await CreateTokenAsync(client))
            {
                var header = new Metadata
                {
                    {"Authorization",$"Bearer {_token}" }
                };
                using var call = client.GetAll(new GetAllRequest(), header);
                var responseSream = call.ResponseStream;
                while (await responseSream.MoveNext())
                {
                    Console.WriteLine(responseSream.Current.Employee);
                }
            }
            
        }

        static async Task AddPhotoAsync(EmployeeService.EmployeeServiceClient client)
        {
            var md = new Metadata
            {
                {"name","dava" },
                {"role","administrator" }
            }; 

             FileStream fs = File.OpenRead( "MVI_5372.AVI");
            using var call = client.AddPhoto(md);

            var stream = call.RequestStream;
            
            while (true)
            {
                byte[] buffer = new byte[16384];
                int numRead = await fs.ReadAsync(buffer,0,buffer.Length);
                if (numRead==0)
                {
                    break;
                }
                if (numRead<buffer.Length)
                {
                    Array.Resize(ref buffer,numRead);
                }

                await stream.WriteAsync(new AddPhotoRequest
                {
                    Data = ByteString.CopyFrom(buffer)
                }) ;
            }

            await stream.CompleteAsync();

            var res = await call.ResponseAsync;
            Console.WriteLine(res.IsOk);
        }

        static async Task Save(EmployeeService.EmployeeServiceClient client)
        {
            var response = await client.SaveAsync(new EmployeeRequest {
                Employee =new Employee
                {
                    Id=9,
                    No=1009,
                    FirstName="ye3",
                    LastName="feng4",
                    Salary=2033,
                    Status=Grpc.Server.Protos.EmployeeStatus.Retired
                }
            });

            Console.WriteLine(response);
        }

        static async Task SaveAll(EmployeeService.EmployeeServiceClient client)
        {
            using var call = client.SaveAll();
            var requestStream = call.RequestStream;
            var responseStream = call.ResponseStream;

            var employees = new List<Employee> { 
                new Employee
                {
                    Id=5,
                    No=1002,
                    FirstName="ye",
                    LastName="feng",
                    Salary=2203
                },
                new Employee
                {
                    Id=6,
                    No=1004,
                    FirstName="liu",
                    LastName="jie",
                    Salary=3443
                },
            };

            var responseTask = Task.Run(async()=> {
                while (await responseStream.MoveNext())
                {
                    Console.WriteLine($"Save Employee {responseStream.Current.Employee}");
                }
            });

            foreach (var employee in employees)
            {
                await requestStream.WriteAsync(new EmployeeRequest { Employee =employee});
            }

            await requestStream.CompleteAsync();
            await responseTask;

        }
    }
}

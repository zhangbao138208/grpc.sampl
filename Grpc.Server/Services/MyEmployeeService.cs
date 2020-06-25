using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Server.Datas;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApplication.Protos;

namespace Grpc.Server.Services
{
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    public class MyEmployeeService:EmployeeService.EmployeeServiceBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MyEmployeeService> _logger;
        private readonly IMapper _mapper;
        private readonly JwtTokenValidationService _jwtTokenValidationService;

        public MyEmployeeService(ILogger<MyEmployeeService> logger,
            ApplicationDbContext applicationDbContext,
            IMapper mapper,
            JwtTokenValidationService jwtTokenValidationService)
        {
            _context = applicationDbContext;
            _logger = logger;
            _mapper = mapper;
            _jwtTokenValidationService = jwtTokenValidationService;
        }
        public async override Task<EmployeeResponse> GetByNo(GetByNoRequest request, ServerCallContext context)
        {
            var md = context.RequestHeaders;
            foreach (var pair in md)
            {
                _logger.LogInformation($"{pair.Key}:{pair.Value}");
            }
            var employee = await _context.Employees.FirstOrDefaultAsync(e=>e.No==request.No);
            if (employee!=null)
            {
                var res = new EmployeeResponse
                {
                    Employee = _mapper.Map<Employee>(employee)
                };
                return await Task.FromResult(res);
            }
            throw new Exception($"employee not found with no:{request.No}");
        }

        public override async Task GetAll(GetAllRequest request, 
            IServerStreamWriter<EmployeeResponse> responseStream, 
            ServerCallContext context)
        {
            foreach (var employee in _context.Employees)
            {
                await responseStream.WriteAsync(new EmployeeResponse { 
                    Employee=_mapper.Map<Employee>(employee)
                });
            }
        }

        public async override Task<AddPhotoResponse> AddPhoto(IAsyncStreamReader<AddPhotoRequest> requestStream, 
            ServerCallContext context)
        {
            var image = await _context.Images.SingleOrDefaultAsync(s=>s.Name== "client.avi");
            Write(Path.Combine(Environment.CurrentDirectory, "dataBase.avi"), image.Data);
            Metadata md = context.RequestHeaders;
            foreach (var pair in md)
            {
                _logger.LogInformation($"{pair.Key}:{pair.Value}");
            }

            var data = new List<byte>();
            while (await requestStream.MoveNext())
            {
                Console.WriteLine($"Received:{requestStream.Current.Data.Length} bytes");
                data.AddRange(requestStream.Current.Data);
            }
            _context.Images.Add(new Models.Image { 
                Data=data.ToArray(),
                Name= "client2.avi"
            });
            await _context.SaveChangesAsync();
            Write(Path.Combine(Environment.CurrentDirectory,"client2.avi"),data.ToArray());
            Console.WriteLine($"Recevied file with {data.Count} bytes");

            return new AddPhotoResponse
            {
                IsOk = true
            };
        }

        public override async Task<EmployeeResponse> Save(EmployeeRequest request,
            ServerCallContext context)
        {
            await  _context.Employees.AddAsync(_mapper.Map<Models.Employee>(request.Employee));
            await _context.SaveChangesAsync();

            Console.WriteLine("Employees:");
            foreach (var employee in _context.Employees.ToList())
            {
                Console.WriteLine(employee);
            }

            return new EmployeeResponse
            {
                Employee = request.Employee
            };

        }

        public override async Task SaveAll(IAsyncStreamReader<EmployeeRequest> requestStream, 
            IServerStreamWriter<EmployeeResponse> responseStream, 
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                lock (this)
                {
                     _context.Employees.Add(_mapper.Map<Models.Employee>(requestStream.Current.Employee));
                }
                await responseStream.WriteAsync(new EmployeeResponse { Employee= requestStream.Current.Employee 
                });
            }
            await _context.SaveChangesAsync();
            Console.WriteLine("Employees:");
            foreach (var employee in _context.Employees.ToList())
            {
                Console.WriteLine(employee);
            }
        }

        [AllowAnonymous]
        public async override Task<TokenResponse> CreateToken(TokenRequest request, 
            ServerCallContext context)
        {
            var userModel = new UserModel
            {
                UserName = request.Username,
                Password = request.Passward
            };
            var response = await _jwtTokenValidationService.GenerateTokenAsync(userModel);
            if (response.Success)
            {
                return new TokenResponse
                {
                    Token = response.Token,
                    Success = true,
                    Expiration = Timestamp.FromDateTime(response.Expiration)
                };
            }
            return new TokenResponse
            {
               
                Success = false,
                
            };

        }
        private void Write(string path,byte[] data)
        {
            FileStream fs = new FileStream(path,FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            // 开始写入
            bw.Write(data,0,data.Length);
            // 关闭流
            bw.Close();
            fs.Close();
        }
    }
}

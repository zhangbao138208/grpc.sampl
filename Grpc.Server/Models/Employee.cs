using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grpc.Server.Models
{
    public class Employee
    {
        public Int32 Id { get; set; }
        public Int32 No { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public float Salary { get; set; }
    }
}

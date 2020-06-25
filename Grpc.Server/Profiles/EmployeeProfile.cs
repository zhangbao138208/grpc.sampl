using AutoMapper;

namespace Grpc.Server.Profiles
{
    public class EmployeeProfile:Profile
    {
        public EmployeeProfile()
        {
            CreateMap<Models.Employee, WebApplication.Protos.Employee>();
            CreateMap<WebApplication.Protos.Employee,Models.Employee>();
        }
    }
}

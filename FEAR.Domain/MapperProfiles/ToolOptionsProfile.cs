using AutoMapper;
using FEAR.Domain.Model;

namespace FEAR.Domain.MapperProfiles
{
    /// <summary>
    /// AutoMapper profile for mapping between Dto.ToolOptions and ToolOptions domain model.
    /// Ignores the ToolOptionId property on the destination to prevent overwriting primary keys during mapping.
    /// </summary>
    public class ToolOptionsProfile : Profile
    {
        public ToolOptionsProfile()
        {
            CreateMap<Dto.ToolOptions, ToolOptions>()
                .ForMember(x => x.ToolOptionId, opt => opt.Ignore());
        }
    }
}

using AutoMapper;
using StudentRegistrar.Models;
using StudentRegistrar.Api.DTOs;

namespace StudentRegistrar.Api.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Legacy Student mappings (for API compatibility)
        CreateMap<LegacyStudent, StudentDto>();
        CreateMap<CreateStudentDto, LegacyStudent>();
        CreateMap<UpdateStudentDto, LegacyStudent>();

        // Legacy Course mappings (for API compatibility)
        CreateMap<LegacyCourse, CourseDto>();
        CreateMap<CreateCourseDto, LegacyCourse>();
        CreateMap<UpdateCourseDto, LegacyCourse>();

        // Legacy Enrollment mappings (for API compatibility)
        CreateMap<LegacyEnrollment, EnrollmentDto>();
        CreateMap<CreateEnrollmentDto, LegacyEnrollment>();

        // Grade record mappings
        CreateMap<GradeRecord, GradeRecordDto>();
        CreateMap<CreateGradeRecordDto, GradeRecord>();

        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.UserProfile));
        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.KeycloakId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.CoursesCreated, opt => opt.Ignore());
        CreateMap<UpdateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.KeycloakId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.CoursesCreated, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // UserProfile mappings
        CreateMap<UserProfile, UserProfileDto>().ReverseMap();
        CreateMap<UserProfileDto, UserProfile>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // CourseInstructor mappings
        CreateMap<CourseInstructor, CourseInstructorDto>()
            .ForMember(dest => dest.InstructorInfo, opt => opt.MapFrom(src => src.GetInstructorInfo()));
        CreateMap<CreateCourseInstructorDto, CourseInstructor>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore())
            .ForMember(dest => dest.InstructorInfoJson, opt => opt.Ignore())
            .AfterMap((src, dest) => 
            {
                if (src.InstructorInfo != null)
                {
                    var modelInfo = new StudentRegistrar.Models.InstructorInfo
                    {
                        Bio = src.InstructorInfo.Bio,
                        Qualifications = src.InstructorInfo.Qualifications,
                        CustomFields = src.InstructorInfo.CustomFields
                    };
                    dest.SetInstructorInfo(modelInfo);
                }
            });
        CreateMap<UpdateCourseInstructorDto, CourseInstructor>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CourseId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Course, opt => opt.Ignore())
            .ForMember(dest => dest.InstructorInfoJson, opt => opt.Ignore())
            .AfterMap((src, dest) => 
            {
                if (src.InstructorInfo != null)
                {
                    var modelInfo = new StudentRegistrar.Models.InstructorInfo
                    {
                        Bio = src.InstructorInfo.Bio,
                        Qualifications = src.InstructorInfo.Qualifications,
                        CustomFields = src.InstructorInfo.CustomFields
                    };
                    dest.SetInstructorInfo(modelInfo);
                }
            });
    }
}

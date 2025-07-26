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
        // AccountHolder mappings
        CreateMap<AccountHolder, AccountHolderDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.AddressJson, opt => opt.MapFrom(src => src.GetAddress()))
            .ForMember(dest => dest.EmergencyContactJson, opt => opt.MapFrom(src => src.GetEmergencyContact()))
            .ForMember(dest => dest.Students, opt => opt.MapFrom(src => src.Students))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments));
            
        CreateMap<CreateAccountHolderDto, AccountHolder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastEdit, opt => opt.Ignore())
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.Payments, opt => opt.Ignore())
            .ForMember(dest => dest.AddressJson, opt => opt.Ignore())
            .ForMember(dest => dest.EmergencyContactJson, opt => opt.Ignore())
            .AfterMap((src, dest, context) => 
            {
                if (src.AddressJson != null)
                {
                    var address = context.Mapper.Map<StudentRegistrar.Models.Address>(src.AddressJson);
                    dest.SetAddress(address);
                }
                if (src.EmergencyContactJson != null)
                {
                    var contact = context.Mapper.Map<StudentRegistrar.Models.EmergencyContact>(src.EmergencyContactJson);
                    dest.SetEmergencyContact(contact);
                }
                dest.MemberSince = DateTime.UtcNow;
                dest.LastEdit = DateTime.UtcNow;
            });
            
        CreateMap<Student, StudentDetailDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.StudentInfoJson, opt => opt.MapFrom(src => src.GetStudentInfo()))
            .ForMember(dest => dest.Enrollments, opt => opt.MapFrom(src => src.Enrollments));
            
        CreateMap<CreateStudentForAccountDto, Student>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AccountHolderId, opt => opt.Ignore())
            .ForMember(dest => dest.AccountHolder, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StudentInfoJson, opt => opt.Ignore())
            .AfterMap((src, dest, context) => 
            {
                if (src.StudentInfoJson != null)
                {
                    var modelInfo = context.Mapper.Map<StudentRegistrar.Models.StudentInfo>(src.StudentInfoJson);
                    dest.SetStudentInfo(modelInfo);
                }
                dest.CreatedAt = DateTime.UtcNow;
                dest.UpdatedAt = DateTime.UtcNow;
            });
            
        CreateMap<Enrollment, EnrollmentDetailDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId.ToString()))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : ""))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Course != null ? src.Course.Code : null))
            .ForMember(dest => dest.SemesterName, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.Name : ""));
            
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
            
        // Supporting object mappings
        CreateMap<StudentRegistrar.Models.Address, AddressInfo>().ReverseMap();
        CreateMap<StudentRegistrar.Models.EmergencyContact, EmergencyContactInfo>().ReverseMap();
        CreateMap<StudentRegistrar.Models.StudentInfo, StudentInfoDetails>().ReverseMap();
    }
}

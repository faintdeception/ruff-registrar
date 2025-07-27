using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Data;
using StudentRegistrar.Data.Repositories;
using StudentRegistrar.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StudentRegistrar.Api.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAccountHolderRepository _accountHolderRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;

    public StudentService(
        IStudentRepository studentRepository,
        IAccountHolderRepository accountHolderRepository,
        IEnrollmentRepository enrollmentRepository,
        IMapper mapper)
    {
        _studentRepository = studentRepository;
        _accountHolderRepository = accountHolderRepository;
        _enrollmentRepository = enrollmentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
    {
        var students = await _studentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<StudentDto>>(students);
    }

    public async Task<StudentDto?> GetStudentByIdAsync(Guid id)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        return student != null ? _mapper.Map<StudentDto>(student) : null;
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        var student = _mapper.Map<Student>(createStudentDto);
        var createdStudent = await _studentRepository.CreateAsync(student);
        return _mapper.Map<StudentDto>(createdStudent);
    }

    public async Task<StudentDto?> UpdateStudentAsync(Guid id, UpdateStudentDto updateStudentDto)
    {
        var existingStudent = await _studentRepository.GetByIdAsync(id);
        if (existingStudent == null)
            return null;

        _mapper.Map(updateStudentDto, existingStudent);
        var updatedStudent = await _studentRepository.UpdateAsync(existingStudent);
        return _mapper.Map<StudentDto>(updatedStudent);
    }

    public async Task<bool> DeleteStudentAsync(Guid id)
    {
        return await _studentRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<EnrollmentDto>> GetStudentEnrollmentsAsync(Guid studentId)
    {
        var enrollments = await _enrollmentRepository.GetByStudentAsync(studentId);
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }

    public async Task<IEnumerable<StudentDto>> GetStudentsByAccountHolderAsync(Guid accountHolderId)
    {
        var students = await _studentRepository.GetByAccountHolderAsync(accountHolderId);
        return _mapper.Map<IEnumerable<StudentDto>>(students);
    }

    // Legacy support methods - will be deprecated
    [Obsolete("Use GetStudentByIdAsync(Guid id) instead")]
    public async Task<StudentDto?> GetStudentByIdAsync(int id)
    {
        // This is a temporary bridge for legacy compatibility
        // In a real migration, you'd need a mapping strategy
        var students = await _studentRepository.GetAllAsync();
        var student = students.FirstOrDefault(s => s.Id.GetHashCode() == id);
        return student != null ? _mapper.Map<StudentDto>(student) : null;
    }

    [Obsolete("Use UpdateStudentAsync(Guid id, UpdateStudentDto) instead")]
    public async Task<StudentDto?> UpdateStudentAsync(int id, UpdateStudentDto updateStudentDto)
    {
        // Legacy bridge - find by hash code
        var students = await _studentRepository.GetAllAsync();
        var student = students.FirstOrDefault(s => s.Id.GetHashCode() == id);
        if (student == null)
            return null;

        return await UpdateStudentAsync(student.Id, updateStudentDto);
    }

    [Obsolete("Use DeleteStudentAsync(Guid id) instead")]
    public async Task<bool> DeleteStudentAsync(int id)
    {
        // Legacy bridge - find by hash code
        var students = await _studentRepository.GetAllAsync();
        var student = students.FirstOrDefault(s => s.Id.GetHashCode() == id);
        if (student == null)
            return false;

        return await DeleteStudentAsync(student.Id);
    }

    [Obsolete("Use GetStudentEnrollmentsAsync(Guid studentId) instead")]
    public async Task<IEnumerable<EnrollmentDto>> GetStudentEnrollmentsAsync(int studentId)
    {
        // Legacy bridge - find by hash code
        var students = await _studentRepository.GetAllAsync();
        var student = students.FirstOrDefault(s => s.Id.GetHashCode() == studentId);
        if (student == null)
            return Enumerable.Empty<EnrollmentDto>();

        return await GetStudentEnrollmentsAsync(student.Id);
    }

    [Obsolete("Use repository pattern instead")]
    public async Task<IEnumerable<GradeRecordDto>> GetStudentGradesAsync(int studentId)
    {
        // This method should be moved to a GradeService when we implement that
        // For now, returning empty to avoid breaking existing code
        return Enumerable.Empty<GradeRecordDto>();
    }
}


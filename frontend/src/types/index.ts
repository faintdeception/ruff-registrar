export interface Student {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  dateOfBirth: string;
  phoneNumber?: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateStudentDto {
  firstName: string;
  lastName: string;
  email: string;
  dateOfBirth: string;
  phoneNumber?: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
}

export interface Course {
  id: number;
  name: string;
  code: string;
  description?: string;
  creditHours: number;
  instructor: string;
  academicYear: string;
  semester: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCourseDto {
  name: string;
  code: string;
  description?: string;
  creditHours: number;
  instructor: string;
  academicYear: string;
  semester: string;
}

export interface Enrollment {
  id: number;
  studentId: number;
  student: Student;
  courseId: number;
  course: Course;
  enrollmentDate: string;
  completionDate?: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEnrollmentDto {
  studentId: number;
  courseId: number;
  enrollmentDate: string;
  status?: string;
}

export interface GradeRecord {
  id: number;
  studentId: number;
  student: Student;
  courseId: number;
  course: Course;
  letterGrade?: string;
  numericGrade?: number;
  gradePoints?: number;
  comments?: string;
  gradeDate: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateGradeRecordDto {
  studentId: number;
  courseId: number;
  letterGrade?: string;
  numericGrade?: number;
  gradePoints?: number;
  comments?: string;
  gradeDate: string;
}

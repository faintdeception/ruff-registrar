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

// Independent Educator types (not tied to courses)
export interface EducatorDto {
  id: string;
  courseId?: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email?: string;
  phone?: string;
  isPrimary: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  educatorInfo: EducatorInfo;
  isAssignedToCourse: boolean;
  course?: any; // Course details if assigned
}

export interface CreateEducatorDto {
  courseId?: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  isPrimary?: boolean;
  isActive?: boolean;
  educatorInfo?: EducatorInfo;
}

export interface UpdateEducatorDto {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  isActive?: boolean;
  educatorInfo?: EducatorInfo;
}

export interface EducatorInfo {
  bio?: string;
  qualifications: string[];
  specializations: string[];
  department?: string;
  customFields: Record<string, string>;
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

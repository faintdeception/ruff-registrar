import Link from 'next/link';
import { useState, useEffect } from 'react';
import { useAuth } from '@/lib/auth';
import ProtectedRoute from '@/components/ProtectedRoute';
import { 
  AcademicCapIcon, 
  UserGroupIcon, 
  BookOpenIcon, 
  ClipboardDocumentListIcon,
  ChartBarIcon,
  ArrowRightOnRectangleIcon,
  UserCircleIcon
} from '@heroicons/react/24/outline';

export default function Home() {
  const { user } = useAuth();
  const [stats, setStats] = useState({
    totalStudents: 0,
    totalCourses: 0,
    totalEnrollments: 0,
    totalGrades: 0,
  });

  useEffect(() => {
    // TODO: Fetch actual stats from API
    // For now, show placeholder data
    setStats({
      totalStudents: 25,
      totalCourses: 12,
      totalEnrollments: 45,
      totalGrades: 38,
    });
  }, []);

  return (
    <ProtectedRoute>
      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Hero Section */}
        <div className="text-center mb-12">
          <h2 className="text-4xl font-bold text-gray-900 mb-4">
            Welcome to Student Registrar
          </h2>
          <p className="text-xl text-gray-600 max-w-2xl mx-auto">
            A comprehensive homeschool management system designed to help you 
            track students, courses, enrollments, and grades with ease.
          </p>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          <div className="card">
            <div className="card-body">
              <div className="flex items-center">
                <UserGroupIcon className="h-8 w-8 text-primary-600" />
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Total Students</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.totalStudents}</p>
                </div>
              </div>
            </div>
          </div>

          <div className="card">
            <div className="card-body">
              <div className="flex items-center">
                <BookOpenIcon className="h-8 w-8 text-primary-600" />
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Total Courses</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.totalCourses}</p>
                </div>
              </div>
            </div>
          </div>

          <div className="card">
            <div className="card-body">
              <div className="flex items-center">
                <ClipboardDocumentListIcon className="h-8 w-8 text-primary-600" />
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Enrollments</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.totalEnrollments}</p>
                </div>
              </div>
            </div>
          </div>

          <div className="card">
            <div className="card-body">
              <div className="flex items-center">
                <ChartBarIcon className="h-8 w-8 text-primary-600" />
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Grades Recorded</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.totalGrades}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <Link href="/account-holder" className="group">
            <div className="card group-hover:shadow-lg transition-shadow">
              <div className="card-body text-center">
                <UserGroupIcon className="h-12 w-12 text-primary-600 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">Manage Students</h3>
                <p className="text-sm text-gray-600">View and manage your students</p>
              </div>
            </div>
          </Link>

          <Link href="/courses" className="group">
            <div className="card group-hover:shadow-lg transition-shadow">
              <div className="card-body text-center">
                <BookOpenIcon className="h-12 w-12 text-primary-600 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">Browse Courses</h3>
                <p className="text-sm text-gray-600">View available courses</p>
              </div>
            </div>
          </Link>

          <Link href="/enrollments" className="group">
            <div className="card group-hover:shadow-lg transition-shadow">
              <div className="card-body text-center">
                <ClipboardDocumentListIcon className="h-12 w-12 text-primary-600 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">Enrollments</h3>
                <p className="text-sm text-gray-600">Manage course enrollments</p>
              </div>
            </div>
          </Link>

          <Link href="/grades" className="group">
            <div className="card group-hover:shadow-lg transition-shadow">
              <div className="card-body text-center">
                <ChartBarIcon className="h-12 w-12 text-primary-600 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">Grades</h3>
                <p className="text-sm text-gray-600">View and manage grades</p>
              </div>
            </div>
          </Link>
        </div>
      </main>
    </ProtectedRoute>
  );
}

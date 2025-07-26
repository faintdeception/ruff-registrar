import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '@/lib/auth';
import ProtectedRoute from '@/components/ProtectedRoute';

interface InstructorInfo {
  bio?: string;
  qualifications: string[];
  customFields: Record<string, string>;
}

interface CourseInstructorDto {
  id: string;
  courseId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email?: string;
  phone?: string;
  isPrimary: boolean;
  createdAt: string;
  updatedAt: string;
  instructorInfo: InstructorInfo;
  course?: {
    id: string;
    name: string;
    code: string;
  };
}

interface CreateCourseInstructorDto {
  courseId: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  isPrimary: boolean;
  instructorInfo?: InstructorInfo;
}

const EducatorsPage = () => {
  const { user } = useAuth();
  const [instructors, setInstructors] = useState<CourseInstructorDto[]>([]);
  const [courses, setCourses] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [showEditForm, setShowEditForm] = useState<string | null>(null);
  const router = useRouter();

  const [newInstructor, setNewInstructor] = useState<CreateCourseInstructorDto>({
    courseId: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    isPrimary: false,
    instructorInfo: {
      bio: '',
      qualifications: [],
      customFields: {}
    }
  });

  const isAdmin = user?.roles?.includes('Administrator');

  useEffect(() => {
    loadInstructors();
    if (isAdmin) {
      loadCourses();
    }
  }, [user]);

  const loadInstructors = async () => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      if (!accessToken) {
        router.push('/login');
        return;
      }

      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/CourseInstructors`, {
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json'
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setInstructors(data);
      } else {
        setError('Failed to load instructors');
      }
    } catch (err) {
      setError('Error loading instructors');
      console.error('Error loading instructors:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const loadCourses = async () => {
    try {
      const accessToken = localStorage.getItem('accessToken');
      if (!accessToken) {
        return;
      }

      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/Courses`, {
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json'
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setCourses(data);
      }
    } catch (err) {
      console.error('Error loading courses:', err);
    }
  };

  const handleAddInstructor = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const accessToken = localStorage.getItem('accessToken');
      if (!accessToken) {
        router.push('/login');
        return;
      }

      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/CourseInstructors`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(newInstructor)
      });

      if (response.ok) {
        const created = await response.json();
        setInstructors([...instructors, created]);
        setShowAddForm(false);
        setNewInstructor({
          courseId: '',
          firstName: '',
          lastName: '',
          email: '',
          phone: '',
          isPrimary: false,
          instructorInfo: {
            bio: '',
            qualifications: [],
            customFields: {}
          }
        });
      } else {
        setError('Failed to create instructor');
      }
    } catch (err) {
      setError('Error creating instructor');
      console.error('Error creating instructor:', err);
    }
  };

  const handleDeactivateInstructor = async (id: string) => {
    if (!isAdmin) return;
    
    try {
      const accessToken = localStorage.getItem('accessToken');
      if (!accessToken) {
        router.push('/login');
        return;
      }

      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/CourseInstructors/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${accessToken}`
        }
      });

      if (response.ok) {
        setInstructors(instructors.filter(i => i.id !== id));
      } else {
        setError('Failed to deactivate instructor');
      }
    } catch (err) {
      setError('Error deactivating instructor');
      console.error('Error deactivating instructor:', err);
    }
  };

  if (isLoading) {
    return (
      <ProtectedRoute>
        <div className="min-h-screen flex items-center justify-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
        </div>
      </ProtectedRoute>
    );
  }

  return (
    <ProtectedRoute>
      <div className="min-h-screen bg-gray-50">
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              Educators
            </h1>
            <p className="text-gray-600">
              View active educators in the system{isAdmin ? ' and manage their assignments' : ''}
            </p>
          </div>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-800">{error}</p>
            </div>
          )}

          {/* Admin Controls */}
          {isAdmin && (
            <div className="mb-6">
              <button
                onClick={() => setShowAddForm(!showAddForm)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                {showAddForm ? 'Cancel' : 'Add New Educator'}
              </button>
            </div>
          )}

          {/* Add Form (Admin Only) */}
          {showAddForm && isAdmin && (
            <div className="mb-8 p-6 bg-white rounded-lg shadow">
              <h2 className="text-xl font-semibold mb-4">Add New Educator</h2>
              <form onSubmit={handleAddInstructor} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Course
                    </label>
                    <select
                      value={newInstructor.courseId}
                      onChange={(e) => setNewInstructor({...newInstructor, courseId: e.target.value})}
                      required
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="">Select Course</option>
                      {courses.map(course => (
                        <option key={course.id} value={course.id}>
                          {course.code} - {course.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      First Name
                    </label>
                    <input
                      type="text"
                      value={newInstructor.firstName}
                      onChange={(e) => setNewInstructor({...newInstructor, firstName: e.target.value})}
                      required
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Last Name
                    </label>
                    <input
                      type="text"
                      value={newInstructor.lastName}
                      onChange={(e) => setNewInstructor({...newInstructor, lastName: e.target.value})}
                      required
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Email
                    </label>
                    <input
                      type="email"
                      value={newInstructor.email}
                      onChange={(e) => setNewInstructor({...newInstructor, email: e.target.value})}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Phone
                    </label>
                    <input
                      type="tel"
                      value={newInstructor.phone}
                      onChange={(e) => setNewInstructor({...newInstructor, phone: e.target.value})}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      id="isPrimary"
                      checked={newInstructor.isPrimary}
                      onChange={(e) => setNewInstructor({...newInstructor, isPrimary: e.target.checked})}
                      className="mr-2"
                    />
                    <label htmlFor="isPrimary" className="text-sm font-medium text-gray-700">
                      Primary Instructor
                    </label>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Bio
                  </label>
                  <textarea
                    value={newInstructor.instructorInfo?.bio || ''}
                    onChange={(e) => setNewInstructor({
                      ...newInstructor,
                      instructorInfo: {
                        ...newInstructor.instructorInfo!,
                        bio: e.target.value
                      }
                    })}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div className="flex justify-end space-x-3">
                  <button
                    type="button"
                    onClick={() => setShowAddForm(false)}
                    className="px-4 py-2 text-gray-600 border border-gray-300 rounded-md hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                  >
                    Add Educator
                  </button>
                </div>
              </form>
            </div>
          )}

          {/* Educators List */}
          <div className="bg-white rounded-lg shadow">
            <div className="px-6 py-4 border-b border-gray-200">
              <h2 className="text-lg font-semibold">Active Educators</h2>
            </div>
            
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Name
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Course
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Contact
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Role
                    </th>
                    {isAdmin && (
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Actions
                      </th>
                    )}
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {instructors.length === 0 ? (
                    <tr>
                      <td colSpan={isAdmin ? 5 : 4} className="px-6 py-4 text-center text-gray-500">
                        No educators found.
                      </td>
                    </tr>
                  ) : (
                    instructors.map((instructor) => (
                      <tr key={instructor.id}>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div>
                            <div className="text-sm font-medium text-gray-900">
                              {instructor.fullName}
                            </div>
                            {instructor.instructorInfo?.bio && (
                              <div className="text-sm text-gray-500">
                                {instructor.instructorInfo.bio.substring(0, 100)}...
                              </div>
                            )}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm text-gray-900">
                            {instructor.course?.name || 'Unknown Course'}
                          </div>
                          <div className="text-sm text-gray-500">
                            {instructor.course?.code}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm text-gray-900">
                            {instructor.email}
                          </div>
                          <div className="text-sm text-gray-500">
                            {instructor.phone}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`px-2 py-1 text-xs font-medium rounded-full ${
                            instructor.isPrimary
                              ? 'bg-blue-100 text-blue-800'
                              : 'bg-gray-100 text-gray-800'
                          }`}>
                            {instructor.isPrimary ? 'Primary' : 'Assistant'}
                          </span>
                        </td>
                        {isAdmin && (
                          <td className="px-6 py-4 whitespace-nowrap">
                            <button
                              onClick={() => handleDeactivateInstructor(instructor.id)}
                              className="text-red-600 hover:text-red-900 text-sm"
                            >
                              Deactivate
                            </button>
                          </td>
                        )}
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {!isAdmin && (
            <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
              <p className="text-blue-800 text-sm">
                <strong>Note:</strong> This page is read-only for members and educators. 
                Only administrators can add new educators or modify existing ones.
              </p>
            </div>
          )}
        </div>
      </div>
    </ProtectedRoute>
  );
};

export default EducatorsPage;

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { useAuth } from '@/lib/auth';
import ProtectedRoute from '@/components/ProtectedRoute';
import {
  BookOpenIcon,
  CalendarIcon,
  UserGroupIcon,
  PlusIcon,
  AcademicCapIcon,
  ClockIcon,
  MapPinIcon,
  CurrencyDollarIcon,
  XMarkIcon
} from '@heroicons/react/24/outline';

interface Semester {
  id: string;
  name: string;
  code: string;
  startDate: string;
  endDate: string;
  registrationStartDate: string;
  registrationEndDate: string;
  isActive: boolean;
  courseCount: number;
}

interface Course {
  id: string;
  name: string;
  code: string;
  description: string;
  room: string;
  maxCapacity: number;
  currentEnrollment: number;
  fee: number;
  periodCode: string;
  ageGroup: string;
  instructorNames?: string[]; // Made optional since courses can be created without instructors
  semesterName: string;
  createdAt: string;
  updatedAt: string;
}

export default function CoursesPage() {
  const { user } = useAuth();
  const [semesters, setSemesters] = useState<Semester[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [selectedSemester, setSelectedSemester] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeSemester, setActiveSemester] = useState<Semester | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [isCreating, setIsCreating] = useState(false);

  const isAdmin = user?.roles.includes('Administrator');

  useEffect(() => {
    fetchSemesters();
  }, []);

  useEffect(() => {
    if (selectedSemester) {
      fetchCoursesBySemester(selectedSemester);
    } else if (activeSemester) {
      fetchCoursesBySemester(activeSemester.id);
    }
  }, [selectedSemester, activeSemester]);

  const fetchSemesters = async () => {
    try {
      setLoading(true);
      
      const token = localStorage.getItem('accessToken');
      if (!token) {
        throw new Error('No authentication token found');
      }

      const semestersResponse = await fetch('/api/semesters', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (!semestersResponse.ok) {
        throw new Error('Failed to fetch semesters');
      }

      const semestersData = await semestersResponse.json();
      setSemesters(semestersData);
      
      // Try to get the active semester
      try {
        const activeSemesterResponse = await fetch('/api/semesters/active', {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        });

        if (activeSemesterResponse.ok) {
          const activeSemesterData = await activeSemesterResponse.json();
          setActiveSemester(activeSemesterData);
          if (!selectedSemester && activeSemesterData) {
            setSelectedSemester(activeSemesterData.id);
          }
        } else {
          // No active semester found, use the first one if available
          if (semestersData.length > 0) {
            setSelectedSemester(semestersData[0].id);
          }
        }
      } catch (err) {
        // No active semester found, use the first one if available
        if (semestersData.length > 0) {
          setSelectedSemester(semestersData[0].id);
        }
      }
    } catch (err) {
      setError('Failed to fetch semesters');
      console.error('Error fetching semesters:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchCoursesBySemester = async (semesterId: string) => {
    try {
      const token = localStorage.getItem('accessToken');
      if (!token) {
        throw new Error('No authentication token found');
      }

      const coursesResponse = await fetch(`/api/newcourses?semesterId=${semesterId}`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (!coursesResponse.ok) {
        throw new Error('Failed to fetch courses');
      }

      const coursesData = await coursesResponse.json();
      setCourses(coursesData);
    } catch (err) {
      setError('Failed to fetch courses');
      console.error('Error fetching courses:', err);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  const createCourse = async (courseData: {
    name: string;
    code?: string;
    description?: string;
    room?: string;
    maxCapacity: number;
    fee: number;
    periodCode?: string;
    startTime?: string;
    endTime?: string;
    ageGroup: string;
  }) => {
    try {
      setIsCreating(true);
      
      const token = localStorage.getItem('accessToken');
      if (!token) {
        throw new Error('No authentication token found');
      }

      if (!selectedSemester) {
        throw new Error('No semester selected');
      }

      // Convert time strings to TimeSpan format if provided
      const createDto = {
        semesterId: selectedSemester,
        name: courseData.name,
        code: courseData.code || null,
        description: courseData.description || null,
        room: courseData.room || null,
        maxCapacity: courseData.maxCapacity,
        fee: courseData.fee,
        periodCode: courseData.periodCode || null,
        startTime: courseData.startTime ? courseData.startTime + ':00' : null,
        endTime: courseData.endTime ? courseData.endTime + ':00' : null,
        ageGroup: courseData.ageGroup
      };

      const response = await fetch('/api/newcourses', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(createDto)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to create course');
      }

      const newCourse = await response.json();
      
      // Refresh the courses list
      await fetchCoursesBySemester(selectedSemester);
      
      setShowCreateModal(false);
      setError(null);
    } catch (err) {
      console.error('Error creating course:', err);
      setError(err instanceof Error ? err.message : 'Failed to create course');
    } finally {
      setIsCreating(false);
    }
  };

  // Course Creation Modal Component
  const CourseCreateModal = () => {
    const [formData, setFormData] = useState({
      name: '',
      code: '',
      description: '',
      room: '',
      maxCapacity: 20,
      fee: 0,
      periodCode: '',
      startTime: '',
      endTime: '',
      ageGroup: ''
    });

    const handleSubmit = async (e: React.FormEvent) => {
      e.preventDefault();
      if (!formData.name || !formData.ageGroup) {
        setError('Course name and age group are required');
        return;
      }
      await createCourse(formData);
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
      const { name, value } = e.target;
      setFormData(prev => ({
        ...prev,
        [name]: name === 'maxCapacity' || name === 'fee' ? Number(value) : value
      }));
    };

    if (!showCreateModal) return null;

    return (
      <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
        <div className="relative top-20 mx-auto p-5 border w-full max-w-2xl shadow-lg rounded-md bg-white">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-medium text-gray-900">Create New Course</h3>
            <button
              onClick={() => setShowCreateModal(false)}
              className="text-gray-400 hover:text-gray-600"
            >
              <span className="sr-only">Close</span>
              <XMarkIcon className="h-6 w-6" />
            </button>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                  Course Name *
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  required
                  value={formData.name}
                  onChange={handleChange}
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  placeholder="e.g., Introduction to Programming"
                />
              </div>

              <div>
                <label htmlFor="code" className="block text-sm font-medium text-gray-700">
                  Course Code
                </label>
                <input
                  type="text"
                  id="code"
                  name="code"
                  value={formData.code}
                  onChange={handleChange}
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  placeholder="e.g., CS101"
                />
              </div>

              <div>
                <label htmlFor="ageGroup" className="block text-sm font-medium text-gray-700">
                  Age Group *
                </label>
                <select
                  id="ageGroup"
                  name="ageGroup"
                  required
                  value={formData.ageGroup}
                  onChange={handleChange}
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                >
                  <option value="">Select age group...</option>
                  <option value="Children">Children (5-12)</option>
                  <option value="Teens">Teens (13-17)</option>
                  <option value="Adults">Adults (18+)</option>
                  <option value="All Ages">All Ages</option>
                </select>
              </div>

              <div>
                <label htmlFor="maxCapacity" className="block text-sm font-medium text-gray-700">
                  Max Capacity
                </label>
                <input
                  type="number"
                  id="maxCapacity"
                  name="maxCapacity"
                  min="1"
                  value={formData.maxCapacity}
                  onChange={handleChange}
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                />
              </div>

              <div>
                <label htmlFor="room" className="block text-sm font-medium text-gray-700">
                  Room
                </label>
                <input
                  type="text"
                  id="room"
                  name="room"
                  value={formData.room}
                  onChange={handleChange}
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  placeholder="e.g., Room 101"
                />
              </div>

              <div>
                <label htmlFor="fee" className="block text-sm font-medium text-gray-700">
                  Course Fee ($)
                </label>
                <input
                  type="number"
                  id="fee"
                  name="fee"
                  min="0"
                  step="0.01"
                  value={formData.fee}
                  onChange={handleChange}
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                />
              </div>

              <div>
                <label htmlFor="periodCode" className="block text-sm font-medium text-gray-700">
                  Period Code
                </label>
                <input
                  type="text"
                  id="periodCode"
                  name="periodCode"
                  value={formData.periodCode}
                  onChange={handleChange}
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                  placeholder="e.g., MW 2:00-3:30 PM"
                />
              </div>

              <div>
                <label htmlFor="startTime" className="block text-sm font-medium text-gray-700">
                  Start Time
                </label>
                <input
                  type="time"
                  id="startTime"
                  name="startTime"
                  value={formData.startTime}
                  onChange={handleChange}
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                />
              </div>

              <div>
                <label htmlFor="endTime" className="block text-sm font-medium text-gray-700">
                  End Time
                </label>
                <input
                  type="time"
                  id="endTime"
                  name="endTime"
                  value={formData.endTime}
                  onChange={handleChange}
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                />
              </div>
            </div>

            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                Description
              </label>
              <textarea
                id="description"
                name="description"
                rows={3}
                value={formData.description}
                onChange={handleChange}
                className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500"
                placeholder="Course description..."
              />
            </div>

            <div className="flex justify-end space-x-3 pt-4">
              <button
                type="button"
                onClick={() => setShowCreateModal(false)}
                className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isCreating}
                className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isCreating ? 'Creating...' : 'Create Course'}
              </button>
            </div>
          </form>
        </div>
      </div>
    );
  };

  if (loading) {
    return (
      <ProtectedRoute>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-primary-600 mx-auto"></div>
            <p className="mt-4 text-gray-600">Loading courses...</p>
          </div>
        </div>
      </ProtectedRoute>
    );
  }

  return (
    <ProtectedRoute>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <div className="bg-white shadow">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <BookOpenIcon className="h-8 w-8 text-primary-600" />
                <h1 className="ml-3 text-2xl font-bold text-gray-900">Courses</h1>
              </div>
              {isAdmin && (
                <div className="flex space-x-3">
                  <Link href="/semesters" className="btn btn-secondary">
                    <CalendarIcon className="h-5 w-5" />
                    Manage Semesters
                  </Link>
                  <button 
                    onClick={() => setShowCreateModal(true)}
                    disabled={!selectedSemester}
                    className="btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <PlusIcon className="h-5 w-5" />
                    Add Course
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {error && (
            <div className="mb-6 bg-red-50 border border-red-200 rounded-md p-4">
              <p className="text-red-600">{error}</p>
            </div>
          )}

          {/* Semester Selection */}
          <div className="mb-8">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-lg font-medium text-gray-900">Select Semester</h2>
                <p className="text-sm text-gray-600">
                  {activeSemester && selectedSemester === activeSemester.id && (
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 mr-2">
                      Active Semester
                    </span>
                  )}
                  View courses for a specific semester period
                </p>
              </div>
              {semesters.length > 0 && (
                <select
                  value={selectedSemester}
                  onChange={(e) => setSelectedSemester(e.target.value)}
                  className="block w-full border-gray-300 rounded-md shadow-sm focus:ring-primary-500 focus:border-primary-500 text-sm"
                >
                  <option value="">Select a semester...</option>
                  {semesters.map((semester) => (
                    <option key={semester.id} value={semester.id}>
                      {semester.name} ({formatDate(semester.startDate)} - {formatDate(semester.endDate)})
                    </option>
                  ))}
                </select>
              )}
            </div>
          </div>

          {/* Current Semester Info */}
          {selectedSemester && (
            <div className="mb-8">
              {(() => {
                const currentSemester = semesters.find(s => s.id === selectedSemester);
                if (!currentSemester) return null;
                
                return (
                  <div className="bg-white rounded-lg shadow p-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <h3 className="text-xl font-semibold text-gray-900">{currentSemester.name}</h3>
                        <p className="text-gray-600">{currentSemester.code}</p>
                      </div>
                      <div className="text-right">
                        <div className="flex items-center text-sm text-gray-600 mb-1">
                          <CalendarIcon className="h-4 w-4 mr-1" />
                          <span>{formatDate(currentSemester.startDate)} - {formatDate(currentSemester.endDate)}</span>
                        </div>
                        <div className="flex items-center text-sm text-gray-600">
                          <BookOpenIcon className="h-4 w-4 mr-1" />
                          <span>{courses.length} course{courses.length !== 1 ? 's' : ''}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })()}
            </div>
          )}

          {/* Courses Grid */}
          {courses.length === 0 && selectedSemester ? (
            <div className="text-center py-12">
              <BookOpenIcon className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">No courses found</h3>
              <p className="text-gray-600 mb-6">
                There are no courses scheduled for the selected semester.
              </p>
              {isAdmin && (
                <button 
                  onClick={() => setShowCreateModal(true)}
                  disabled={!selectedSemester}
                  className="btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <PlusIcon className="h-5 w-5" />
                  Add First Course
                </button>
              )}
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {courses.map((course) => (
                <div key={course.id} className="bg-white rounded-lg shadow hover:shadow-md transition-shadow">
                  <div className="p-6">
                    {/* Course Header */}
                    <div className="mb-4">
                      <div className="flex items-start justify-between">
                        <div className="flex-1 min-w-0">
                          <h3 className="text-lg font-semibold text-gray-900 truncate">
                            {course.name}
                          </h3>
                          {course.code && (
                            <p className="text-sm text-gray-600 font-mono">{course.code}</p>
                          )}
                        </div>
                        <span className="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                          {course.ageGroup}
                        </span>
                      </div>
                      {course.description && (
                        <p className="mt-2 text-sm text-gray-600 line-clamp-2">
                          {course.description}
                        </p>
                      )}
                    </div>

                    {/* Course Details */}
                    <div className="space-y-2 mb-4">
                      {course.room && (
                        <div className="flex items-center text-sm text-gray-600">
                          <MapPinIcon className="h-4 w-4 mr-2 flex-shrink-0" />
                          <span>{course.room}</span>
                        </div>
                      )}
                      
                      {course.periodCode && (
                        <div className="flex items-center text-sm text-gray-600">
                          <ClockIcon className="h-4 w-4 mr-2 flex-shrink-0" />
                          <span>{course.periodCode}</span>
                        </div>
                      )}

                      <div className="flex items-center text-sm text-gray-600">
                        <UserGroupIcon className="h-4 w-4 mr-2 flex-shrink-0" />
                        <span>
                          {course.currentEnrollment} / {course.maxCapacity} students
                        </span>
                      </div>

                      {course.fee > 0 && (
                        <div className="flex items-center text-sm text-gray-600">
                          <CurrencyDollarIcon className="h-4 w-4 mr-2 flex-shrink-0" />
                          <span>{formatCurrency(course.fee)}</span>
                        </div>
                      )}
                    </div>

                    {/* Instructors */}
                    {course.instructorNames && course.instructorNames.length > 0 && (
                      <div className="mb-4">
                        <div className="flex items-center text-sm text-gray-600 mb-1">
                          <AcademicCapIcon className="h-4 w-4 mr-2" />
                          <span>Instructor{course.instructorNames.length > 1 ? 's' : ''}:</span>
                        </div>
                        <div className="pl-6">
                          {course.instructorNames?.map((name, index) => (
                            <span key={index} className="text-sm text-gray-700">
                              {name}
                              {index < (course.instructorNames?.length || 0) - 1 && ', '}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Progress Bar */}
                    <div className="mb-4">
                      <div className="flex items-center justify-between text-sm text-gray-600 mb-1">
                        <span>Enrollment</span>
                        <span>{Math.round((course.currentEnrollment / course.maxCapacity) * 100)}%</span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div
                          className={`h-2 rounded-full ${
                            course.currentEnrollment >= course.maxCapacity
                              ? 'bg-red-500'
                              : course.currentEnrollment >= course.maxCapacity * 0.8
                              ? 'bg-yellow-500'
                              : 'bg-green-500'
                          }`}
                          style={{
                            width: `${Math.min((course.currentEnrollment / course.maxCapacity) * 100, 100)}%`
                          }}
                        ></div>
                      </div>
                    </div>

                    {/* Actions */}
                    <div className="flex space-x-2">
                      <button className="flex-1 btn btn-secondary text-sm py-2">
                        View Details
                      </button>
                      {isAdmin && (
                        <button className="btn btn-secondary text-sm py-2 px-3">
                          Edit
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Course Creation Modal */}
        <CourseCreateModal />
      </div>
    </ProtectedRoute>
  );
}

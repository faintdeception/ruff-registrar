import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '../lib/auth';
import ProtectedRoute from '../components/ProtectedRoute';

interface AccountHolder {
  id: string;
  firstName: string;
  lastName: string;
  emailAddress: string;
  homePhone?: string;
  mobilePhone?: string;
  addressJson: {
    street: string;
    city: string;
    state: string;
    postalCode: string;
    country: string;
  };
  emergencyContactJson: {
    firstName: string;
    lastName: string;
    homePhone?: string;
    mobilePhone?: string;
    email: string;
  };
  membershipDuesOwed: number;
  membershipDuesReceived: number;
  memberSince: string;
  lastLogin?: string;
  lastEdit: string;
  students: Student[];
  payments: Payment[];
}

interface Student {
  id: string;
  firstName: string;
  lastName: string;
  grade?: string;
  dateOfBirth?: string;
  studentInfoJson: {
    specialConditions?: string[];
    allergies?: string[];
    medications?: string[];
    preferredName?: string;
    parentNotes?: string;
  };
  notes?: string;
  enrollments: Enrollment[];
}

interface Enrollment {
  id: string;
  courseId: string;
  courseName: string;
  courseCode?: string;
  semesterName: string;
  enrollmentType: string;
  enrollmentDate: string;
  feeAmount: number;
  amountPaid: number;
  paymentStatus: string;
  waitlistPosition?: number;
  notes?: string;
}

interface Payment {
  id: string;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  paymentType: string;
  transactionId?: string;
  notes?: string;
}

const AccountHolderPage: React.FC = () => {
  const { user } = useAuth();
  const router = useRouter();
  const [accountHolder, setAccountHolder] = useState<AccountHolder | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!user) return;

    const fetchAccountHolder = async () => {
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          throw new Error('No authentication token found');
        }

        const response = await fetch(`/api/account-holders/me`, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        });

        if (!response.ok) {
          throw new Error('Failed to fetch account holder data');
        }

        const data = await response.json();
        setAccountHolder(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchAccountHolder();
  }, [user]);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="bg-red-50 border border-red-200 rounded-md p-4">
          <div className="text-red-800">
            <h3 className="text-lg font-medium">Error</h3>
            <p className="mt-1">{error}</p>
          </div>
        </div>
      </div>
    );
  }

  if (!accountHolder) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-gray-500">No account holder data found</div>
      </div>
    );
  }

  const totalDuesBalance = accountHolder.membershipDuesOwed - accountHolder.membershipDuesReceived;

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="bg-white shadow rounded-lg mb-6">
          <div className="px-4 py-5 sm:px-6">
            <div className="flex justify-between items-start">
              <div>
                <h1 className="text-2xl font-bold text-gray-900">
                  {accountHolder.firstName} {accountHolder.lastName}
                </h1>
                <p className="text-sm text-gray-500">
                  Member since {formatDate(accountHolder.memberSince)}
                </p>
              </div>
              <div className="text-right">
                <p className="text-sm text-gray-500">Last Login</p>
                <p className="text-sm font-medium">
                  {accountHolder.lastLogin ? formatDate(accountHolder.lastLogin) : 'Never'}
                </p>
              </div>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Account Information */}
          <div className="lg:col-span-2">
            <div className="bg-white shadow rounded-lg">
              <div className="px-4 py-5 sm:px-6">
                <h2 className="text-lg font-medium text-gray-900">Account Information</h2>
              </div>
              <div className="border-t border-gray-200">
                <dl className="divide-y divide-gray-200">
                  <div className="px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Email</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      {accountHolder.emailAddress}
                    </dd>
                  </div>
                  <div className="px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Phone Numbers</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      {accountHolder.homePhone && (
                        <div>Home: {accountHolder.homePhone}</div>
                      )}
                      {accountHolder.mobilePhone && (
                        <div>Mobile: {accountHolder.mobilePhone}</div>
                      )}
                    </dd>
                  </div>
                  <div className="px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Address</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      <div>{accountHolder.addressJson.street}</div>
                      <div>
                        {accountHolder.addressJson.city}, {accountHolder.addressJson.state} {accountHolder.addressJson.postalCode}
                      </div>
                      <div>{accountHolder.addressJson.country}</div>
                    </dd>
                  </div>
                  <div className="px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Emergency Contact</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      <div className="font-medium">
                        {accountHolder.emergencyContactJson.firstName} {accountHolder.emergencyContactJson.lastName}
                      </div>
                      <div>{accountHolder.emergencyContactJson.email}</div>
                      {accountHolder.emergencyContactJson.homePhone && (
                        <div>Home: {accountHolder.emergencyContactJson.homePhone}</div>
                      )}
                      {accountHolder.emergencyContactJson.mobilePhone && (
                        <div>Mobile: {accountHolder.emergencyContactJson.mobilePhone}</div>
                      )}
                    </dd>
                  </div>
                </dl>
              </div>
            </div>
          </div>

          {/* Membership Dues */}
          <div className="space-y-6">
            <div className="bg-white shadow rounded-lg">
              <div className="px-4 py-5 sm:px-6">
                <h2 className="text-lg font-medium text-gray-900">Membership Dues</h2>
              </div>
              <div className="border-t border-gray-200 px-4 py-5 sm:px-6">
                <dl className="space-y-3">
                  <div className="flex justify-between">
                    <dt className="text-sm text-gray-500">Total Owed</dt>
                    <dd className="text-sm font-medium text-gray-900">
                      {formatCurrency(accountHolder.membershipDuesOwed)}
                    </dd>
                  </div>
                  <div className="flex justify-between">
                    <dt className="text-sm text-gray-500">Total Received</dt>
                    <dd className="text-sm font-medium text-gray-900">
                      {formatCurrency(accountHolder.membershipDuesReceived)}
                    </dd>
                  </div>
                  <div className="flex justify-between border-t border-gray-200 pt-3">
                    <dt className="text-sm font-medium text-gray-500">Balance</dt>
                    <dd className={`text-sm font-medium ${
                      totalDuesBalance > 0 ? 'text-red-600' : 'text-green-600'
                    }`}>
                      {formatCurrency(totalDuesBalance)}
                    </dd>
                  </div>
                </dl>
              </div>
            </div>
          </div>
        </div>

        {/* Students */}
        {accountHolder.students && accountHolder.students.length > 0 && (
          <div className="mt-6">
            <div className="bg-white shadow rounded-lg">
              <div className="px-4 py-5 sm:px-6">
                <h2 className="text-lg font-medium text-gray-900">Students</h2>
              </div>
              <div className="border-t border-gray-200">
                <div className="space-y-6 px-4 py-5 sm:px-6">
                  {accountHolder.students.map((student) => (
                    <div key={student.id} className="border border-gray-200 rounded-lg p-4">
                      <div className="flex justify-between items-start mb-3">
                        <div>
                          <h3 className="text-lg font-medium text-gray-900">
                            {student.firstName} {student.lastName}
                            {student.studentInfoJson.preferredName && 
                              student.studentInfoJson.preferredName !== student.firstName && 
                              ` (${student.studentInfoJson.preferredName})`
                            }
                          </h3>
                          {student.grade && (
                            <p className="text-sm text-gray-500">Grade: {student.grade}</p>
                          )}
                          {student.dateOfBirth && (
                            <p className="text-sm text-gray-500">
                              Birth Date: {formatDate(student.dateOfBirth)}
                            </p>
                          )}
                        </div>
                      </div>

                      {/* Student Info */}
                      {(student.studentInfoJson.allergies?.length || 
                        student.studentInfoJson.medications?.length || 
                        student.studentInfoJson.specialConditions?.length) && (
                        <div className="mb-3 p-3 bg-yellow-50 rounded">
                          <h4 className="text-sm font-medium text-yellow-800 mb-2">Important Information</h4>
                          {student.studentInfoJson.allergies?.length && (
                            <p className="text-sm text-yellow-700">
                              <strong>Allergies:</strong> {student.studentInfoJson.allergies.join(', ')}
                            </p>
                          )}
                          {student.studentInfoJson.medications?.length && (
                            <p className="text-sm text-yellow-700">
                              <strong>Medications:</strong> {student.studentInfoJson.medications.join(', ')}
                            </p>
                          )}
                          {student.studentInfoJson.specialConditions?.length && (
                            <p className="text-sm text-yellow-700">
                              <strong>Special Conditions:</strong> {student.studentInfoJson.specialConditions.join(', ')}
                            </p>
                          )}
                        </div>
                      )}

                      {/* Enrollments */}
                      {student.enrollments && student.enrollments.length > 0 && (
                        <div>
                          <h4 className="text-sm font-medium text-gray-900 mb-2">Current Enrollments</h4>
                          <div className="space-y-2">
                            {student.enrollments.map((enrollment) => (
                              <div key={enrollment.id} className="flex justify-between items-center p-2 bg-gray-50 rounded">
                                <div>
                                  <span className="text-sm font-medium">{enrollment.courseName}</span>
                                  {enrollment.courseCode && (
                                    <span className="text-sm text-gray-500 ml-2">({enrollment.courseCode})</span>
                                  )}
                                  <div className="text-xs text-gray-500">
                                    {enrollment.semesterName} • {enrollment.enrollmentType}
                                    {enrollment.waitlistPosition && (
                                      <span className="text-yellow-600"> • Waitlist #{enrollment.waitlistPosition}</span>
                                    )}
                                  </div>
                                </div>
                                <div className="text-right">
                                  <div className="text-sm font-medium">
                                    {formatCurrency(enrollment.feeAmount)}
                                  </div>
                                  <div className={`text-xs ${
                                    enrollment.paymentStatus === 'PAID' ? 'text-green-600' : 
                                    enrollment.paymentStatus === 'PARTIAL' ? 'text-yellow-600' : 
                                    'text-red-600'
                                  }`}>
                                    {enrollment.paymentStatus}
                                  </div>
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Recent Payments */}
        {accountHolder.payments && accountHolder.payments.length > 0 && (
          <div className="mt-6">
            <div className="bg-white shadow rounded-lg">
              <div className="px-4 py-5 sm:px-6">
                <h2 className="text-lg font-medium text-gray-900">Recent Payments</h2>
              </div>
              <div className="border-t border-gray-200">
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Date
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Amount
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Method
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Type
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                          Transaction ID
                        </th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {accountHolder.payments.slice(0, 10).map((payment) => (
                        <tr key={payment.id}>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                            {formatDate(payment.paymentDate)}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                            {formatCurrency(payment.amount)}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {payment.paymentMethod}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {payment.paymentType}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {payment.transactionId || '-'}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default function AccountHolder() {
  return (
    <ProtectedRoute>
      <AccountHolderPage />
    </ProtectedRoute>
  );
}

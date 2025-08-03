import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/router';
import { useAuth } from '../lib/auth';
import ProtectedRoute from '../components/ProtectedRoute';
import { PlusIcon, XMarkIcon, UserIcon, PhoneIcon, EnvelopeIcon, MapPinIcon } from '@heroicons/react/24/outline';

interface Member {
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
  students: any[];
  payments: any[];
}

interface CreateMemberForm {
  firstName: string;
  lastName: string;
  emailAddress: string;
  homePhone: string;
  mobilePhone: string;
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
    homePhone: string;
    mobilePhone: string;
    email: string;
  };
}

const MembersPage: React.FC = () => {
  const { user } = useAuth();
  const router = useRouter();
  const [members, setMembers] = useState<Member[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [creating, setCreating] = useState(false);
  const [newMember, setNewMember] = useState<CreateMemberForm>({
    firstName: '',
    lastName: '',
    emailAddress: '',
    homePhone: '',
    mobilePhone: '',
    addressJson: {
      street: '',
      city: '',
      state: '',
      postalCode: '',
      country: 'US'
    },
    emergencyContactJson: {
      firstName: '',
      lastName: '',
      homePhone: '',
      mobilePhone: '',
      email: ''
    }
  });

  useEffect(() => {
    if (!user) return;
    
    // Only admins can view this page
    if (!user.roles?.includes('Administrator')) {
      router.push('/');
      return;
    }
    
    fetchMembers();
  }, [user, router]);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const fetchMembers = async () => {
    try {
      setError(null);
      setSuccessMessage(null);
      setLoading(true);
      
      const token = localStorage.getItem('accessToken');
      if (!token) {
        throw new Error('No authentication token found');
      }

      const response = await fetch('/api/members', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to fetch members');
      }

      const data = await response.json();
      setMembers(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch members');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateMember = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setError(null);
      setSuccessMessage(null);
      setCreating(true);
      
      const token = localStorage.getItem('accessToken');
      if (!token) {
        throw new Error('No authentication token found');
      }

      const response = await fetch('/api/members', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(newMember),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to create member');
      }

      // Refresh members list
      fetchMembers();
      
      // Show success message
      setSuccessMessage('Member created successfully!');
      setTimeout(() => setSuccessMessage(null), 5000);
      
      // Reset form
      setNewMember({
        firstName: '',
        lastName: '',
        emailAddress: '',
        homePhone: '',
        mobilePhone: '',
        addressJson: {
          street: '',
          city: '',
          state: '',
          postalCode: '',
          country: 'US'
        },
        emergencyContactJson: {
          firstName: '',
          lastName: '',
          homePhone: '',
          mobilePhone: '',
          email: ''
        }
      });
      setShowCreateForm(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create member');
    } finally {
      setCreating(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-gray-500">Loading members...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        {/* Success Message */}
        {successMessage && (
          <div id="success-message" data-testid="success-message" className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg">
            <p className="text-green-800">{successMessage}</p>
          </div>
        )}
        
        {/* Error Message */}
        {error && (
          <div id="error-message" className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800">{error}</p>
          </div>
        )}
        
        {/* Header */}
        <div className="bg-white shadow rounded-lg mb-6">
          <div className="px-4 py-5 sm:px-6 flex justify-between items-center">
            <div>
              <h3 className="text-lg leading-6 font-medium text-gray-900">Members Management</h3>
              <p className="mt-1 max-w-2xl text-sm text-gray-500">Manage member accounts and information</p>
            </div>
            <button
              id="create-member-button"
              onClick={() => setShowCreateForm(true)}
              className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              <PlusIcon className="h-4 w-4 mr-2" />
              Create Member
            </button>
          </div>
        </div>

        {/* Create Member Form */}
        {showCreateForm && (
          <div className="bg-white shadow rounded-lg mb-6">
            <div className="px-4 py-5 sm:px-6">
              <div className="flex justify-between items-center mb-4">
                <h4 className="text-lg font-medium text-gray-900">Create New Member</h4>
                <button
                  onClick={() => setShowCreateForm(false)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <XMarkIcon className="h-6 w-6" />
                </button>
              </div>
              
              <form id="create-member-form" onSubmit={handleCreateMember} className="space-y-6">
                {/* Basic Information */}
                <div>
                  <h5 className="text-md font-medium text-gray-900 mb-3">Basic Information</h5>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        First Name *
                      </label>
                      <input
                        id="member-first-name"
                        type="text"
                        required
                        value={newMember.firstName}
                        onChange={(e) => setNewMember({...newMember, firstName: e.target.value})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Last Name *
                      </label>
                      <input
                        id="member-last-name"
                        type="text"
                        required
                        value={newMember.lastName}
                        onChange={(e) => setNewMember({...newMember, lastName: e.target.value})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Email Address *
                      </label>
                      <input
                        id="member-email"
                        type="email"
                        required
                        value={newMember.emailAddress}
                        onChange={(e) => setNewMember({...newMember, emailAddress: e.target.value})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Mobile Phone
                      </label>
                      <input
                        id="member-mobile-phone"
                        type="tel"
                        value={newMember.mobilePhone}
                        onChange={(e) => setNewMember({...newMember, mobilePhone: e.target.value})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Home Phone
                      </label>
                      <input
                        id="member-home-phone"
                        type="tel"
                        value={newMember.homePhone}
                        onChange={(e) => setNewMember({...newMember, homePhone: e.target.value})}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                  </div>
                </div>

                {/* Address Information */}
                <div>
                  <h5 className="text-md font-medium text-gray-900 mb-3">Address Information</h5>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="md:col-span-2">
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Street Address
                      </label>
                      <input
                        id="member-street"
                        type="text"
                        value={newMember.addressJson.street}
                        onChange={(e) => setNewMember({
                          ...newMember, 
                          addressJson: {...newMember.addressJson, street: e.target.value}
                        })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        City
                      </label>
                      <input
                        id="member-city"
                        type="text"
                        value={newMember.addressJson.city}
                        onChange={(e) => setNewMember({
                          ...newMember, 
                          addressJson: {...newMember.addressJson, city: e.target.value}
                        })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        State
                      </label>
                      <input
                        id="member-state"
                        type="text"
                        value={newMember.addressJson.state}
                        onChange={(e) => setNewMember({
                          ...newMember, 
                          addressJson: {...newMember.addressJson, state: e.target.value}
                        })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Postal Code
                      </label>
                      <input
                        id="member-postal-code"
                        type="text"
                        value={newMember.addressJson.postalCode}
                        onChange={(e) => setNewMember({
                          ...newMember, 
                          addressJson: {...newMember.addressJson, postalCode: e.target.value}
                        })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                  </div>
                </div>

                {/* Emergency Contact */}
                <div>
                  <h5 className="text-md font-medium text-gray-900 mb-3">Emergency Contact</h5>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Emergency Contact First Name
                      </label>
                      <input
                        id="emergency-first-name"
                        type="text"
                        value={newMember.emergencyContactJson.firstName}
                        onChange={(e) => setNewMember({
                          ...newMember, 
                          emergencyContactJson: {...newMember.emergencyContactJson, firstName: e.target.value}
                        })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Emergency Contact Last Name
                      </label>
                      <input
                        id="emergency-last-name"
                        type="text"
                        value={newMember.emergencyContactJson.lastName}
                        onChange={(e) => setNewMember({
                          ...newMember, 
                          emergencyContactJson: {...newMember.emergencyContactJson, lastName: e.target.value}
                        })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Emergency Contact Email
                      </label>
                      <input
                        id="emergency-email"
                        type="email"
                        value={newMember.emergencyContactJson.email}
                        onChange={(e) => setNewMember({
                          ...newMember, 
                          emergencyContactJson: {...newMember.emergencyContactJson, email: e.target.value}
                        })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Emergency Contact Phone
                      </label>
                      <input
                        id="emergency-mobile-phone"
                        type="tel"
                        value={newMember.emergencyContactJson.mobilePhone}
                        onChange={(e) => setNewMember({
                          ...newMember, 
                          emergencyContactJson: {...newMember.emergencyContactJson, mobilePhone: e.target.value}
                        })}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                  </div>
                </div>
                
                <div className="flex justify-end space-x-3">
                  <button
                    id="cancel-create-member"
                    type="button"
                    onClick={() => setShowCreateForm(false)}
                    className="px-4 py-2 text-gray-600 border border-gray-300 rounded-md hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    id="submit-create-member"
                    type="submit"
                    disabled={creating}
                    className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
                  >
                    {creating ? 'Creating...' : 'Create Member'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Members List */}
        <div className="bg-white shadow rounded-lg">
          <div className="px-4 py-5 sm:px-6">
            <h4 className="text-lg font-medium text-gray-900">All Members ({members.length})</h4>
          </div>
          
          {members.length === 0 ? (
            <div className="text-center py-8">
              <UserIcon className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-500">No members found</p>
            </div>
          ) : (
            <div className="overflow-hidden">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 p-6">
                {members.map((member) => (
                  <div key={member.id} className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
                    {/* Member Name and Status */}
                    <div className="flex items-center justify-between mb-3">
                      <h5 className="text-lg font-medium text-gray-900">
                        {member.firstName} {member.lastName}
                      </h5>
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                        Active
                      </span>
                    </div>

                    {/* Contact Information */}
                    <div className="space-y-2 mb-4">
                      <div className="flex items-center text-sm text-gray-600">
                        <EnvelopeIcon className="h-4 w-4 mr-2" />
                        {member.emailAddress}
                      </div>
                      {member.mobilePhone && (
                        <div className="flex items-center text-sm text-gray-600">
                          <PhoneIcon className="h-4 w-4 mr-2" />
                          {member.mobilePhone}
                        </div>
                      )}
                      {(member.addressJson.city || member.addressJson.state) && (
                        <div className="flex items-center text-sm text-gray-600">
                          <MapPinIcon className="h-4 w-4 mr-2" />
                          {member.addressJson.city}{member.addressJson.city && member.addressJson.state && ', '}{member.addressJson.state}
                        </div>
                      )}
                    </div>

                    {/* Member Statistics */}
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="text-gray-500">Students:</span>
                        <span className="ml-1 font-medium">{member.students.length}</span>
                      </div>
                      <div>
                        <span className="text-gray-500">Since:</span>
                        <span className="ml-1 font-medium">{formatDate(member.memberSince)}</span>
                      </div>
                      <div className="col-span-2">
                        <span className="text-gray-500">Dues Balance:</span>
                        <span className={`ml-1 font-medium ${member.membershipDuesOwed - member.membershipDuesReceived > 0 ? 'text-red-600' : 'text-green-600'}`}>
                          {formatCurrency(member.membershipDuesOwed - member.membershipDuesReceived)}
                        </span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default function Members() {
  return (
    <ProtectedRoute>
      <MembersPage />
    </ProtectedRoute>
  );
}

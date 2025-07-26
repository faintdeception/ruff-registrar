import { NextApiRequest, NextApiResponse } from 'next';

interface JwtPayload {
  email?: string;
  preferred_username?: string;
  realm_access?: {
    roles?: string[];
  };
}

function decodeJwt(token: string): JwtPayload | null {
  try {
    // Simple JWT decode without verification (for demo purposes)
    const payload = token.split('.')[1];
    const decoded = JSON.parse(Buffer.from(payload, 'base64').toString());
    return decoded;
  } catch (error) {
    return null;
  }
}

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  if (req.method !== 'GET') {
    return res.status(405).json({ message: 'Method not allowed' });
  }

  // Get token from Authorization header
  const authHeader = req.headers.authorization;
  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    return res.status(401).json({ message: 'No token provided' });
  }

  const token = authHeader.substring(7);

  try {
    // Decode the JWT token (note: in production, you should verify the signature)
    const decoded = decodeJwt(token);
    
    if (!decoded || !decoded.email) {
      return res.status(401).json({ message: 'Invalid token' });
    }

    const userEmail = decoded.email;
    const userRoles = decoded.realm_access?.roles || [];

    // Check if user is scoopmember (should see John Smith family data)
    if (userEmail === 'scoopmember@example.com') {
      const mockAccountHolder = {
        id: '92261fe2-4612-4067-abe1-6ce5e3a650e4',
        firstName: 'John',
        lastName: 'Smith',
        emailAddress: 'scoopmember@example.com',
        homePhone: '555-0101',
        mobilePhone: '555-0102',
        addressJson: {
          street: '123 Main St',
          city: 'Anytown',
          state: 'CA',
          postalCode: '12345',
          country: 'US'
        },
        emergencyContactJson: {
          firstName: 'Jane',
          lastName: 'Smith',
          homePhone: '555-0103',
          mobilePhone: '555-0104',
          email: 'jane.smith@example.com'
        },
        membershipDuesOwed: 100.00,
        membershipDuesReceived: 75.00,
        memberSince: '2024-01-15T00:00:00Z',
        lastLogin: '2025-07-16T10:30:00Z',
        lastEdit: '2025-07-16T11:13:13Z',
        students: [
          {
            id: 'student-1',
            firstName: 'Emma',
            lastName: 'Smith',
            grade: '3',
            dateOfBirth: '2016-04-15T00:00:00Z',
            studentInfoJson: {
              specialConditions: ['Allergic to peanuts'],
              allergies: ['Peanuts', 'Tree nuts'],
              medications: [],
              preferredName: 'Em',
              parentNotes: 'Very outgoing child, loves art'
            },
            notes: 'Enrolled in art classes',
            enrollments: [
              {
                id: 'enrollment-1',
                courseId: 'course-1',
                courseName: 'Creative Arts',
                courseCode: 'ART101',
                semesterName: 'Fall 2025',
                enrollmentType: 'ENROLLED',
                enrollmentDate: '2025-07-16T00:00:00Z',
                feeAmount: 75.00,
                amountPaid: 75.00,
                paymentStatus: 'PAID',
                waitlistPosition: null,
                notes: null
              }
            ]
          },
          {
            id: 'student-2',
            firstName: 'Liam',
            lastName: 'Smith',
            grade: '1',
            dateOfBirth: '2018-09-22T00:00:00Z',
            studentInfoJson: {
              specialConditions: [],
              allergies: [],
              medications: [],
              preferredName: 'Liam',
              parentNotes: 'Shy but loves science'
            },
            notes: 'Interested in science experiments',
            enrollments: [
              {
                id: 'enrollment-2',
                courseId: 'course-2',
                courseName: 'Science Exploration',
                courseCode: 'SCI101',
                semesterName: 'Fall 2025',
                enrollmentType: 'ENROLLED',
                enrollmentDate: '2025-07-16T00:00:00Z',
                feeAmount: 65.00,
                amountPaid: 0.00,
                paymentStatus: 'PENDING',
                waitlistPosition: null,
                notes: null
              }
            ]
          }
        ],
        payments: [
          {
            id: 'payment-1',
            amount: 75.00,
            paymentDate: '2025-07-16T00:00:00Z',
            paymentMethod: 'CASH',
            paymentType: 'COURSE_FEE',
            transactionId: 'TXN001',
            notes: 'Payment for Emma art class'
          },
          {
            id: 'payment-2',
            amount: 75.00,
            paymentDate: '2025-07-15T00:00:00Z',
            paymentMethod: 'CHECK',
            paymentType: 'MEMBERSHIP_DUES',
            transactionId: 'CHK001',
            notes: 'Membership dues payment'
          }
        ]
      };

      return res.status(200).json(mockAccountHolder);
    }

    // Check if user is admin - admins might not have AccountHolder records
    if (userRoles.includes('Administrator')) {
      // For admins, either show their account holder data if they have one,
      // or show a message that they don't have account holder data
      return res.status(404).json({ 
        message: 'No account holder data found. As an administrator, you may not have a family account.' 
      });
    }

    // Default case - no account holder data found
    return res.status(404).json({ 
      message: 'No account holder data found for this user.' 
    });

  } catch (error) {
    console.error('Error processing request:', error);
    return res.status(500).json({ message: 'Internal server error' });
  }
}

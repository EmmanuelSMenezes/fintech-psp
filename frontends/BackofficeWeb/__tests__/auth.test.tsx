import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import { AuthProvider, useAuth } from '@/context/AuthContext';
import SignInPage from '@/app/(full-width-pages)/auth/signin/page';

// Mock do Next.js router
jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

// Mock do react-hot-toast
jest.mock('react-hot-toast', () => ({
  __esModule: true,
  default: {
    success: jest.fn(),
    error: jest.fn(),
  },
}));

// Mock do axios
jest.mock('axios', () => ({
  create: jest.fn(() => ({
    post: jest.fn(),
    interceptors: {
      request: { use: jest.fn() },
      response: { use: jest.fn() },
    },
  })),
}));

const mockPush = jest.fn();
const mockRouter = useRouter as jest.MockedFunction<typeof useRouter>;

describe('Authentication', () => {
  beforeEach(() => {
    mockRouter.mockReturnValue({
      push: mockPush,
      replace: jest.fn(),
      prefetch: jest.fn(),
      back: jest.fn(),
      forward: jest.fn(),
      refresh: jest.fn(),
    } as any);
    mockPush.mockClear();
  });

  describe('SignInPage', () => {
    it('should render login form', () => {
      render(
        <AuthProvider>
          <SignInPage />
        </AuthProvider>
      );

      expect(screen.getByText('FintechPSP')).toBeInTheDocument();
      expect(screen.getByText('Backoffice - Acesso Administrativo')).toBeInTheDocument();
      expect(screen.getByLabelText('Client ID')).toBeInTheDocument();
      expect(screen.getByLabelText('Client Secret')).toBeInTheDocument();
      expect(screen.getByLabelText('Tipo de Acesso')).toBeInTheDocument();
    });

    it('should show validation errors for empty fields', async () => {
      render(
        <AuthProvider>
          <SignInPage />
        </AuthProvider>
      );

      const submitButton = screen.getByRole('button', { name: /acessar backoffice/i });
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText('Client ID é obrigatório')).toBeInTheDocument();
        expect(screen.getByText('Client Secret é obrigatório')).toBeInTheDocument();
      });
    });

    it('should fill admin credentials when admin button is clicked', () => {
      render(
        <AuthProvider>
          <SignInPage />
        </AuthProvider>
      );

      const adminButton = screen.getByText('Admin');
      fireEvent.click(adminButton);

      const clientIdInput = screen.getByLabelText('Client ID') as HTMLInputElement;
      const clientSecretInput = screen.getByLabelText('Client Secret') as HTMLInputElement;
      const scopeSelect = screen.getByLabelText('Tipo de Acesso') as HTMLSelectElement;

      expect(clientIdInput.value).toBe('admin_backoffice');
      expect(clientSecretInput.value).toBe('admin_secret_000');
      expect(scopeSelect.value).toBe('admin');
    });

    it('should fill sub-admin credentials when sub-admin button is clicked', () => {
      render(
        <AuthProvider>
          <SignInPage />
        </AuthProvider>
      );

      const subAdminButton = screen.getByText('Sub-Admin');
      fireEvent.click(subAdminButton);

      const clientIdInput = screen.getByLabelText('Client ID') as HTMLInputElement;
      const clientSecretInput = screen.getByLabelText('Client Secret') as HTMLInputElement;
      const scopeSelect = screen.getByLabelText('Tipo de Acesso') as HTMLSelectElement;

      expect(clientIdInput.value).toBe('sub_admin_backoffice');
      expect(clientSecretInput.value).toBe('sub_admin_secret_000');
      expect(scopeSelect.value).toBe('sub-admin');
    });
  });

  describe('AuthContext', () => {
    const TestComponent = () => {
      const { user, isAuthenticated, hasPermission } = useAuth();
      
      return (
        <div>
          <div data-testid="authenticated">{isAuthenticated.toString()}</div>
          <div data-testid="user-email">{user?.email || 'No user'}</div>
          <div data-testid="user-role">{user?.role || 'No role'}</div>
          <div data-testid="has-view-dashboard">{hasPermission('view_dashboard').toString()}</div>
          <div data-testid="has-manage-system">{hasPermission('manage_system').toString()}</div>
        </div>
      );
    };

    it('should provide authentication context', () => {
      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      expect(screen.getByTestId('authenticated')).toHaveTextContent('false');
      expect(screen.getByTestId('user-email')).toHaveTextContent('No user');
      expect(screen.getByTestId('user-role')).toHaveTextContent('No role');
      expect(screen.getByTestId('has-view-dashboard')).toHaveTextContent('false');
      expect(screen.getByTestId('has-manage-system')).toHaveTextContent('false');
    });
  });
});

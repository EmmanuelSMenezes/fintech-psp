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

describe('Internet Banking Authentication', () => {
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
    it('should render login form for internet banking', () => {
      render(
        <AuthProvider>
          <SignInPage />
        </AuthProvider>
      );

      expect(screen.getByText('FintechPSP')).toBeInTheDocument();
      expect(screen.getByText('Internet Banking - Acesso Cliente')).toBeInTheDocument();
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

      const submitButton = screen.getByRole('button', { name: /acessar internet banking/i });
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText('Client ID é obrigatório')).toBeInTheDocument();
        expect(screen.getByText('Client Secret é obrigatório')).toBeInTheDocument();
      });
    });

    it('should fill cliente credentials when cliente button is clicked', () => {
      render(
        <AuthProvider>
          <SignInPage />
        </AuthProvider>
      );

      const clienteButton = screen.getByText('Cliente');
      fireEvent.click(clienteButton);

      const clientIdInput = screen.getByLabelText('Client ID') as HTMLInputElement;
      const clientSecretInput = screen.getByLabelText('Client Secret') as HTMLInputElement;
      const scopeSelect = screen.getByLabelText('Tipo de Acesso') as HTMLSelectElement;

      expect(clientIdInput.value).toBe('cliente_banking');
      expect(clientSecretInput.value).toBe('cliente_secret_000');
      expect(scopeSelect.value).toBe('banking');
    });

    it('should fill sub-cliente credentials when sub-cliente button is clicked', () => {
      render(
        <AuthProvider>
          <SignInPage />
        </AuthProvider>
      );

      const subClienteButton = screen.getByText('Sub-Cliente');
      fireEvent.click(subClienteButton);

      const clientIdInput = screen.getByLabelText('Client ID') as HTMLInputElement;
      const clientSecretInput = screen.getByLabelText('Client Secret') as HTMLInputElement;
      const scopeSelect = screen.getByLabelText('Tipo de Acesso') as HTMLSelectElement;

      expect(clientIdInput.value).toBe('sub_cliente_banking');
      expect(clientSecretInput.value).toBe('sub_cliente_secret_000');
      expect(scopeSelect.value).toBe('sub-banking');
    });

    it('should show internet banking features info', () => {
      render(
        <AuthProvider>
          <SignInPage />
        </AuthProvider>
      );

      expect(screen.getByText('Funcionalidades disponíveis:')).toBeInTheDocument();
      expect(screen.getByText('• Dashboard pessoal com resumo financeiro')).toBeInTheDocument();
      expect(screen.getByText('• Gerenciamento de contas bancárias')).toBeInTheDocument();
      expect(screen.getByText('• Histórico de transações')).toBeInTheDocument();
      expect(screen.getByText('• Configuração de priorização')).toBeInTheDocument();
      expect(screen.getByText('• Gestão de sub-usuários')).toBeInTheDocument();
    });
  });

  describe('AuthContext for Banking', () => {
    const TestComponent = () => {
      const { user, isAuthenticated, hasPermission } = useAuth();
      
      return (
        <div>
          <div data-testid="authenticated">{isAuthenticated.toString()}</div>
          <div data-testid="user-email">{user?.email || 'No user'}</div>
          <div data-testid="user-role">{user?.role || 'No role'}</div>
          <div data-testid="has-view-dashboard">{hasPermission('view_dashboard').toString()}</div>
          <div data-testid="has-create-transactions">{hasPermission('create_transactions').toString()}</div>
          <div data-testid="has-manage-sub-users">{hasPermission('manage_sub_users').toString()}</div>
        </div>
      );
    };

    it('should provide banking authentication context', () => {
      render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      expect(screen.getByTestId('authenticated')).toHaveTextContent('false');
      expect(screen.getByTestId('user-email')).toHaveTextContent('No user');
      expect(screen.getByTestId('user-role')).toHaveTextContent('No role');
      expect(screen.getByTestId('has-view-dashboard')).toHaveTextContent('false');
      expect(screen.getByTestId('has-create-transactions')).toHaveTextContent('false');
      expect(screen.getByTestId('has-manage-sub-users')).toHaveTextContent('false');
    });
  });
});

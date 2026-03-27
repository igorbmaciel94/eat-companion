import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Input } from '../../../components/ui/Input';
import { Button } from '../../../components/ui/Button';

export function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    // Mock login — navigate to dashboard
    navigate('/');
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <Input
        label="Email"
        type="email"
        placeholder="you@example.com"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        required
      />
      <Input
        label="Password"
        type="password"
        placeholder="Enter your password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        required
      />
      <Button type="submit" fullWidth size="lg" className="mt-4">
        Login
      </Button>
      <p className="text-center text-sm text-on-surface-variant mt-6">
        Don't have an account?{' '}
        <Link to="/register" className="text-primary font-medium">
          Register
        </Link>
      </p>
    </form>
  );
}

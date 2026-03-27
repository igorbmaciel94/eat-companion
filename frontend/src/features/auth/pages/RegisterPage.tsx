import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Input } from '../../../components/ui/Input';
import { Button } from '../../../components/ui/Button';

export function RegisterPage() {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    navigate('/welcome');
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <Input
        label="Name"
        type="text"
        placeholder="Your full name"
        value={name}
        onChange={(e) => setName(e.target.value)}
        required
      />
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
        placeholder="Create a password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        required
      />
      <Button type="submit" fullWidth size="lg" className="mt-4">
        Create Account
      </Button>
      <p className="text-center text-sm text-on-surface-variant mt-6">
        Already have an account?{' '}
        <Link to="/login" className="text-primary font-medium">
          Login
        </Link>
      </p>
    </form>
  );
}

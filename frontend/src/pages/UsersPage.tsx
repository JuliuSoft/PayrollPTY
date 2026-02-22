import { useEffect, useState } from 'react';
import api from '../api/client';

export default function UsersPage() {
  const [users, setUsers] = useState<any[]>([]);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState('HR');

  const load = () => api.get('/users').then(r => setUsers(r.data));
  useEffect(() => { load(); }, []);

  const create = async () => {
    await api.post('/users', { username, password, role });
    setUsername(''); setPassword(''); setRole('HR');
    await load();
  };

  return <div>
    <h2>Users (Admin)</h2>
    <div className='card'>
      <input placeholder='Username' value={username} onChange={e => setUsername(e.target.value)} />
      <input placeholder='Password' type='password' value={password} onChange={e => setPassword(e.target.value)} />
      <select value={role} onChange={e => setRole(e.target.value)}>
        <option>PayrollAdmin</option><option>HR</option><option>Finance</option><option>Auditor</option>
      </select>
      <button onClick={create}>Create User</button>
    </div>
    <div className='card'>
      <table className='table'><thead><tr><th>Username</th><th>Role</th></tr></thead><tbody>{users.map(u => <tr key={u.id}><td>{u.username}</td><td>{u.role}</td></tr>)}</tbody></table>
    </div>
  </div>;
}

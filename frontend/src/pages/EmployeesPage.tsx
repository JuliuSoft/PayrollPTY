import { useEffect, useState } from 'react';
import api from '../api/client';

export default function EmployeesPage() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState({ fullName: '', nationalId: '', department: '', bankAccount: '', baseSalary: 1200 });

  const load = () => api.get('/employees').then(r => setItems(r.data));
  useEffect(() => { load().catch(() => setError('Could not load employees.')).finally(() => setLoading(false)); }, []);

  const create = async () => {
    await api.post('/employees', {
      fullName: form.fullName,
      nationalId: form.nationalId,
      hireDate: '2025-01-01',
      department: form.department,
      status: 0,
      bankAccount: form.bankAccount,
      salaryType: 0,
      baseSalary: Number(form.baseSalary),
      hourlyRate: 0,
      taxConfig: 'default'
    });
    setForm({ fullName: '', nationalId: '', department: '', bankAccount: '', baseSalary: 1200 });
    await load();
  };

  return <div>
    <h2>Employees</h2>
    <div className='card'>
      <h4>Data Entry</h4>
      <input placeholder='Full name' value={form.fullName} onChange={e => setForm({ ...form, fullName: e.target.value })} />
      <input placeholder='National ID' value={form.nationalId} onChange={e => setForm({ ...form, nationalId: e.target.value })} />
      <input placeholder='Department' value={form.department} onChange={e => setForm({ ...form, department: e.target.value })} />
      <input placeholder='Bank account' value={form.bankAccount} onChange={e => setForm({ ...form, bankAccount: e.target.value })} />
      <input type='number' placeholder='Base salary' value={form.baseSalary} onChange={e => setForm({ ...form, baseSalary: Number(e.target.value) })} />
      <button onClick={create}>Add Employee</button>
    </div>
    <div className='card'>
      {loading && <p>Loading employees...</p>}
      {error && <p className='error'>{error}</p>}
      {!loading && !error && <table className='table'>
        <thead><tr><th>Name</th><th>NationalId</th><th>Department</th><th>Status</th><th>Salary</th></tr></thead>
        <tbody>{items.map(e => <tr key={e.id}><td>{e.fullName}</td><td>{e.nationalId}</td><td>{e.department || '-'}</td><td>{e.status}</td><td>{e.baseSalary}</td></tr>)}</tbody>
      </table>}
    </div>
  </div>;
}

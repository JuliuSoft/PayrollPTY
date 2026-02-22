import { useEffect, useState } from 'react';
import api from '../api/client';

export default function EmployeesPage() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    api.get('/employees')
      .then(r => setItems(r.data))
      .catch(() => setError('Could not load employees.'))
      .finally(() => setLoading(false));
  }, []);

  return <div>
    <h2>Employees</h2>
    <div className='card'>
      {loading && <p>Loading employees...</p>}
      {error && <p className='error'>{error}</p>}
      {!loading && !error && <table className='table'>
        <thead><tr><th>Name</th><th>Department</th><th>Status</th><th>Salary</th></tr></thead>
        <tbody>{items.map(e => <tr key={e.id}><td>{e.fullName}</td><td>{e.department || '-'}</td><td>{e.status}</td><td>{e.baseSalary}</td></tr>)}</tbody>
      </table>}
    </div>
  </div>;
}

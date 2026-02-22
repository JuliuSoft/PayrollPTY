import { useEffect, useState } from 'react';
import api from '../api/client';

export default function DashboardPage() {
  const [stats, setStats] = useState({ employees: 0, periods: 0, runs: 0 });

  useEffect(() => {
    Promise.all([api.get('/employees'), api.get('/pay-periods'), api.get('/payroll-runs')])
      .then(([e, p, r]) => setStats({ employees: e.data.length, periods: p.data.length, runs: r.data.length }))
      .catch(() => setStats({ employees: 0, periods: 0, runs: 0 }));
  }, []);

  return <div>
    <h2>Dashboard</h2>
    <div className='grid3'>
      <div className='card'><h4>Employees</h4><strong>{stats.employees}</strong></div>
      <div className='card'><h4>Pay Periods</h4><strong>{stats.periods}</strong></div>
      <div className='card'><h4>Payroll Runs</h4><strong>{stats.runs}</strong></div>
    </div>
  </div>;
}

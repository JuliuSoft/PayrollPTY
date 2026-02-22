import { useEffect, useState } from 'react';
import api from '../api/client';

export default function PayrollRunsPage() {
  const [runs, setRuns] = useState<any[]>([]);

  const load = () => api.get('/payroll-runs').then(r => setRuns(r.data));
  useEffect(() => { load(); }, []);

  const preview = async (id: string) => {
    await api.post(`/payroll-runs/${id}/preview`);
    await load();
  };

  return <div>
    <h2>Payroll Runs</h2>
    <div className='card'>
      <table className='table'>
        <thead><tr><th>Run Type</th><th>Status</th><th>Created By</th><th>Created At</th><th>Action</th></tr></thead>
        <tbody>
          {runs.map(r => <tr key={r.id}>
            <td>{r.runType}</td>
            <td><span className='badge'>{r.status}</span></td>
            <td>{r.createdBy}</td>
            <td>{new Date(r.createdAt).toLocaleString()}</td>
            <td><button onClick={() => preview(r.id)} disabled={r.status === 'Locked'}>Preview</button></td>
          </tr>)}
        </tbody>
      </table>
    </div>
  </div>;
}

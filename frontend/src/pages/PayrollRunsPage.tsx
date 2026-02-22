import { useEffect, useState } from 'react';
import axios from 'axios';
import api from '../api/client';

export default function PayrollRunsPage() {
  const [runs, setRuns] = useState<any[]>([]);
  const [selectedRunId, setSelectedRunId] = useState('');
  const [results, setResults] = useState<any[]>([]);
  const [loadingRun, setLoadingRun] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const loadRuns = () => api.get('/payroll-runs').then(r => setRuns(r.data));

  const statusLabel = (status: any) => {
    const map: Record<string, string> = { '0': 'Draft', '1': 'Validated', '2': 'Preview', '3': 'Approved', '4': 'Locked' };
    return map[String(status)] || String(status);
  };

  const isLocked = (status: any) => String(status) === '4' || String(status).toLowerCase() === 'locked';

  const loadResults = async (runId: string) => {
    const r = await api.get(`/payroll-runs/${runId}/results`);
    setResults(r.data);
    setSelectedRunId(runId);
  };

  useEffect(() => { loadRuns(); }, []);

  const preview = async (id: string) => {
    setLoadingRun(id);
    setMessage('');
    setError('');
    try {
      const res = await api.post(`/payroll-runs/${id}/preview`);
      await loadRuns();
      await loadResults(id);
      setMessage(`Preview generated for run ${id}. Results: ${res.data?.results ?? results.length}`);
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const status = err.response?.status;
        const apiError = (err.response?.data as any)?.error;
        setError(`Preview failed${status ? ` (${status})` : ''}${apiError ? `: ${apiError}` : ''}`);
      } else {
        setError('Preview failed due to an unexpected error.');
      }
    } finally {
      setLoadingRun('');
    }
  };

  return <div>
    <h2>Payroll Runs</h2>
    <div className='card'>
      {message && <p className='success'>{message}</p>}
      {error && <p className='error'>{error}</p>}
      <table className='table'>
        <thead><tr><th>Run Type</th><th>Status</th><th>Created By</th><th>Created At</th><th>Action</th><th>Details</th></tr></thead>
        <tbody>
          {runs.map(r => <tr key={r.id}>
            <td>{r.runType}</td>
            <td><span className='badge'>{statusLabel(r.status)}</span></td>
            <td>{r.createdBy}</td>
            <td>{new Date(r.createdAt).toLocaleString()}</td>
            <td>
              <button onClick={() => preview(r.id)} disabled={isLocked(r.status) || loadingRun === r.id}>
                {loadingRun === r.id ? 'Processing...' : 'Preview'}
              </button>
            </td>
            <td><button onClick={() => loadResults(r.id)}>View Results</button></td>
          </tr>)}
        </tbody>
      </table>
    </div>

    <div className='card'>
      <h4>Run Results {selectedRunId ? `(${selectedRunId})` : ''}</h4>
      {results.length === 0 ? <p>No results loaded. Click "View Results" or run Preview.</p> :
        <table className='table'>
          <thead><tr><th>EmployeeId</th><th>Gross</th><th>Deductions</th><th>Net</th><th>Employer Cost</th></tr></thead>
          <tbody>{results.map((x:any) => <tr key={x.id}><td>{x.employeeId}</td><td>{x.gross}</td><td>{x.deductions}</td><td>{x.net}</td><td>{x.employerCostTotal}</td></tr>)}</tbody>
        </table>
      }
    </div>
  </div>;
}

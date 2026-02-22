import { useEffect, useMemo, useState } from 'react';
import { Bar, BarChart, CartesianGrid, Cell, Line, LineChart, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import api from '../api/client';

type Period = { id: string; startDate: string; endDate: string };

const normalize = (rows: any[]) => rows.map(r => ({ ...r, totalCost: Number(r.totalCost ?? 0), gross: Number(r.gross ?? 0), net: Number(r.net ?? 0), amount: Number(r.amount ?? 0) }));

export default function ReportsPage() {
  const [from, setFrom] = useState('2025-01-01');
  const [to, setTo] = useState('2026-12-31');
  const [periodId, setPeriodId] = useState('');
  const [periods, setPeriods] = useState<Period[]>([]);
  const [cost, setCost] = useState<any[]>([]);
  const [grossNet, setGrossNet] = useState<any[]>([]);
  const [deductions, setDeductions] = useState<any[]>([]);
  const [top, setTop] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const csvUrl = useMemo(() => `${api.defaults.baseURL}/reports/payroll-cost/csv?from=${from}&to=${to}`, [from, to]);

  useEffect(() => {
    api.get('/pay-periods')
      .then(r => {
        const data = r.data as Period[];
        setPeriods(data);
        if (data.length && !periodId) setPeriodId(data[0].id);
      })
      .catch(() => setError('Could not load pay periods for report filters.'));
  }, []);

  const load = async () => {
    setLoading(true);
    setError('');
    try {
      const c = await api.get('/reports/payroll-cost', { params: { from, to } });
      setCost(normalize(c.data));

      const g = await api.get('/reports/gross-vs-net', { params: { from, to } });
      setGrossNet(normalize(g.data));

      if (periodId) {
        const d = await api.get('/reports/deductions-breakdown', { params: { periodId } });
        setDeductions(normalize(d.data));

        const t = await api.get('/reports/top-earners', { params: { periodId, top: 10 } });
        setTop(normalize(t.data));
      } else {
        setDeductions([]);
        setTop([]);
      }
    } catch {
      setError('Could not load report data. Verify backend is running and your user has Finance/Auditor/PayrollAdmin role.');
      setCost([]);
      setGrossNet([]);
      setDeductions([]);
      setTop([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { if (periodId) load(); }, [periodId]);

  return <div>
    <h2>Reports</h2>
    <div className='card'>
      <strong>Filters</strong><br />
      From <input type='date' value={from} onChange={e => setFrom(e.target.value)} />
      To <input type='date' value={to} onChange={e => setTo(e.target.value)} />
      Period
      <select value={periodId} onChange={e => setPeriodId(e.target.value)}>
        <option value=''>Select period</option>
        {periods.map(p => <option key={p.id} value={p.id}>{p.startDate} → {p.endDate}</option>)}
      </select>
      <button onClick={load} disabled={loading}>{loading ? 'Loading...' : 'Apply'}</button>
      <button onClick={() => window.open(csvUrl, '_blank')}>Download CSV</button>
      {error && <p className='error'>{error}</p>}
    </div>

    <div className='card'><h4>Total Payroll Cost</h4>{cost.length === 0 ? <p>No data for selected range.</p> : <ResponsiveContainer width='100%' height={240}><LineChart data={cost}><CartesianGrid strokeDasharray='3 3' /><XAxis dataKey='period' /><YAxis /><Tooltip /><Line type='monotone' dataKey='totalCost' stroke='#1d4ed8' /></LineChart></ResponsiveContainer>}</div>
    <div className='card'><h4>Gross vs Net</h4>{grossNet.length === 0 ? <p>No data for selected range.</p> : <ResponsiveContainer width='100%' height={240}><BarChart data={grossNet}><CartesianGrid strokeDasharray='3 3' /><XAxis dataKey='period' /><YAxis /><Tooltip /><Bar dataKey='gross' fill='#334155' /><Bar dataKey='net' fill='#22c55e' /></BarChart></ResponsiveContainer>}</div>
    <div className='grid2'>
      <div className='card'><h4>Deductions Breakdown</h4>{deductions.length === 0 ? <p>No deduction data for selected period.</p> : <ResponsiveContainer width='100%' height={240}><PieChart><Pie data={deductions} dataKey='amount' nameKey='code' outerRadius={90}>{deductions.map((_: any, i: number) => <Cell key={i} fill={['#ef4444', '#f59e0b', '#6366f1'][i % 3]} />)}</Pie><Tooltip /></PieChart></ResponsiveContainer>}</div>
      <div className='card'><h4>Top 10 Earners</h4>{top.length === 0 ? <p>No top-earner data for selected period.</p> : <ResponsiveContainer width='100%' height={240}><BarChart data={top}><CartesianGrid strokeDasharray='3 3' /><XAxis dataKey='employee' /><YAxis /><Tooltip /><Bar dataKey='gross' fill='#8b5cf6' /></BarChart></ResponsiveContainer>}</div>
    </div>
  </div>;
}

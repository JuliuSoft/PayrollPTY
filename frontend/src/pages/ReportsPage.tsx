import { useEffect, useState } from 'react';
import { Bar, BarChart, CartesianGrid, Cell, Line, LineChart, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import api from '../api/client';

export default function ReportsPage(){
  const [from,setFrom]=useState('2025-01-01'); const [to,setTo]=useState('2025-12-31');
  const [cost,setCost]=useState<any[]>([]); const [grossNet,setGrossNet]=useState<any[]>([]); const [deductions,setDeductions]=useState<any[]>([]); const [top,setTop]=useState<any[]>([]);
  const [periodId,setPeriodId]=useState('');
  const load=async()=>{ const c=await api.get('/reports/payroll-cost',{params:{from,to}});setCost(c.data); const g=await api.get('/reports/gross-vs-net',{params:{from,to}});setGrossNet(g.data); if(periodId){setDeductions((await api.get('/reports/deductions-breakdown',{params:{periodId}})).data); setTop((await api.get('/reports/top-earners',{params:{periodId,top:10}})).data);} };
  useEffect(()=>{load();},[periodId]);
  return <div><h2>Reports</h2><div className='card'>From <input value={from} onChange={e=>setFrom(e.target.value)}/> To <input value={to} onChange={e=>setTo(e.target.value)}/> PeriodId <input value={periodId} onChange={e=>setPeriodId(e.target.value)}/> <button onClick={load}>Apply</button> <button onClick={()=>window.open(`http://localhost:5000/reports/payroll-cost/csv?from=${from}&to=${to}`)}>Download CSV</button></div>
  <div className='card'><h4>Total Payroll Cost</h4><ResponsiveContainer width='100%' height={240}><LineChart data={cost}><CartesianGrid strokeDasharray='3 3'/><XAxis dataKey='period'/><YAxis/><Tooltip/><Line type='monotone' dataKey='totalCost' stroke='#1d4ed8'/></LineChart></ResponsiveContainer></div>
  <div className='card'><h4>Gross vs Net</h4><ResponsiveContainer width='100%' height={240}><BarChart data={grossNet}><CartesianGrid strokeDasharray='3 3'/><XAxis dataKey='period'/><YAxis/><Tooltip/><Bar dataKey='gross' fill='#334155'/><Bar dataKey='net' fill='#22c55e'/></BarChart></ResponsiveContainer></div>
  <div className='card'><h4>Deductions Breakdown</h4><ResponsiveContainer width='100%' height={240}><PieChart><Pie data={deductions} dataKey='amount' nameKey='code' outerRadius={90}>{deductions.map((_:any,i:number)=><Cell key={i} fill={['#ef4444','#f59e0b','#6366f1'][i%3]}/> )}</Pie><Tooltip/></PieChart></ResponsiveContainer></div>
  <div className='card'><h4>Top 10 Earners</h4><ResponsiveContainer width='100%' height={240}><BarChart data={top}><CartesianGrid strokeDasharray='3 3'/><XAxis dataKey='employee'/><YAxis/><Tooltip/><Bar dataKey='gross' fill='#8b5cf6'/></BarChart></ResponsiveContainer></div>
  </div>
}

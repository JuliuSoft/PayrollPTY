import { useEffect, useState } from 'react';
import api from '../api/client';
export default function PayrollRunsPage(){const [runs,setRuns]=useState<any[]>([]);useEffect(()=>{api.get('/payroll-runs').then(r=>setRuns(r.data));},[]);const preview=async(id:string)=>{await api.post(`/payroll-runs/${id}/preview`);alert('Preview generated')};return <div><h2>Payroll Runs</h2><div className='card'>{runs.map(r=><div key={r.id}>{r.runType} <span className='badge'>{r.status}</span> <button onClick={()=>preview(r.id)}>Preview</button></div>)}</div></div>}

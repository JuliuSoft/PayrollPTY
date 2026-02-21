import { useEffect, useState } from 'react';
import api from '../api/client';
export default function PayPeriodsPage(){const [items,setItems]=useState<any[]>([]);useEffect(()=>{api.get('/pay-periods').then(r=>setItems(r.data));},[]);return <div><h2>Pay Periods</h2><div className='card'>{items.map(p=><div key={p.id}>{p.startDate} - {p.endDate}</div>)}</div></div>}

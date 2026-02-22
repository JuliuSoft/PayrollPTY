import { useEffect, useState } from 'react';
import api from '../api/client';
export default function EmployeesPage(){const [items,setItems]=useState<any[]>([]);useEffect(()=>{api.get('/employees').then(r=>setItems(r.data));},[]);return <div><h2>Employees</h2><div className='card'>{items.map(e=><div key={e.id}>{e.fullName} - {e.department}</div>)}</div></div>}

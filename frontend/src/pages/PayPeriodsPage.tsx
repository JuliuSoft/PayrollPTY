import { useEffect, useState } from 'react';
import api from '../api/client';

export default function PayPeriodsPage() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get('/pay-periods').then(r => setItems(r.data)).finally(() => setLoading(false));
  }, []);

  return <div>
    <h2>Pay Periods</h2>
    <div className='card'>
      {loading ? <p>Loading periods...</p> : <table className='table'>
        <thead><tr><th>Start</th><th>End</th><th>Pay Date</th><th>Status</th></tr></thead>
        <tbody>{items.map(p => <tr key={p.id}><td>{p.startDate}</td><td>{p.endDate}</td><td>{p.payDate}</td><td>{p.status}</td></tr>)}</tbody>
      </table>}
    </div>
  </div>;
}

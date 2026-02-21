import { Link, Outlet } from 'react-router-dom';
export default function MainLayout(){return <div className='layout'><aside className='sidebar'><h3>PayrollPTY</h3><Link to='/'>Dashboard</Link><Link to='/employees'>Employees</Link><Link to='/periods'>Pay Periods</Link><Link to='/runs'>Payroll Runs</Link><Link to='/reports'>Reports</Link></aside><main className='content'><Outlet/></main></div>}

import { Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider, useAuth } from './auth/AuthContext';
import MainLayout from './layouts/MainLayout';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import EmployeesPage from './pages/EmployeesPage';
import PayPeriodsPage from './pages/PayPeriodsPage';
import PayrollRunsPage from './pages/PayrollRunsPage';
import ReportsPage from './pages/ReportsPage';
import UsersPage from './pages/UsersPage';

const Guard = ({children}:{children:JSX.Element}) => { const {user}=useAuth(); return user?children:<Navigate to='/login'/>};
export default function App(){return <AuthProvider><Routes><Route path='/login' element={<LoginPage/>}/><Route element={<Guard><MainLayout/></Guard>}><Route path='/' element={<DashboardPage/>}/><Route path='/employees' element={<EmployeesPage/>}/><Route path='/periods' element={<PayPeriodsPage/>}/><Route path='/runs' element={<PayrollRunsPage/>}/><Route path='/reports' element={<ReportsPage/>}/><Route path='/users' element={<UsersPage/>}/></Route></Routes></AuthProvider>}

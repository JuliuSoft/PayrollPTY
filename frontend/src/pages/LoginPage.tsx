import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
export default function LoginPage(){const {login}=useAuth();const nav=useNavigate();const [u,setU]=useState('admin');const [p,setP]=useState('Admin123!');return <div className='content'><div className='card'><h2>Login</h2><input value={u} onChange={e=>setU(e.target.value)}/><input value={p} onChange={e=>setP(e.target.value)} type='password'/><button onClick={async()=>{await login(u,p);nav('/')}}>Sign in</button></div></div>}

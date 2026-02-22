import { createContext, useContext, useState } from 'react';
import api from '../api/client';
const AuthContext = createContext<any>(null);
export const AuthProvider = ({children}:{children:React.ReactNode}) => { const [user,setUser]=useState<string|null>(localStorage.getItem('user')); const login=async(username:string,password:string)=>{const {data}=await api.post('/auth/login',{username,password});localStorage.setItem('token',data.token);localStorage.setItem('user',username);setUser(username)}; const logout=()=>{localStorage.clear();setUser(null)}; return <AuthContext.Provider value={{user,login,logout}}>{children}</AuthContext.Provider>};
export const useAuth=()=>useContext(AuthContext);

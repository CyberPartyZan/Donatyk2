import { useEffect, useState } from 'react';
import { HomePage } from './pages/HomePage';
import { Route, Routes } from 'react-router';
import './App.css';
import { LotFull } from './components/LotFull';

function App() {
  return (
    <Routes>
      <Route index element={<HomePage />} />
      <Route path='lot' element={<LotFull />} />
    </Routes>
  );
}

export default App;

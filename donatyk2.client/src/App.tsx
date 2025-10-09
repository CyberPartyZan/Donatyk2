import { useEffect, useState } from 'react';
import { HomePage } from './pages/HomePage';
import { Route, Routes } from 'react-router';
import './App.css';

function App() {
  return (
    <Routes>
      <Route index element={<HomePage />} />
    </Routes>
  );
}

export default App;

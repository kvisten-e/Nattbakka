import React from "react";
import TransactionList from "./TransactionGroups";
import logo from "./images/bat1.png"; 
import logoText from "./images/nattbakka_text_transp.png"; 
import "./App.css";
import SavedCards from "./SavedCards";

function App() {
  return (
    <div className="App">
      <div className="header-images">
        <img src={logo} alt="Logo 1" className="header-logo" />
        <img src={logoText} alt="Logo 2" className="header-logo" />
      </div>
      <SavedCards />
      <TransactionList />
    </div>
  );
}

export default App;

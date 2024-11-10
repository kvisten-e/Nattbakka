import React, { useState, useEffect } from "react";
import TransactionCard from "./TransactionCard"; // Assuming TransactionCard is in the same folder

const SavedCards = () => {
  const [savedCards, setSavedCards] = useState(
    JSON.parse(localStorage.getItem("savedCards")) || []
  );

  const handleDrop = (event) => {
    const group = JSON.parse(event.dataTransfer.getData("cardData"));
    if (!savedCards.find(card => card.id === group.id)) {
      const updatedSavedCards = [...savedCards, group];
      setSavedCards(updatedSavedCards);
      localStorage.setItem("savedCards", JSON.stringify(updatedSavedCards));
    }
    event.preventDefault();
  };

  const allowDrop = (event) => event.preventDefault();

  const handleDeleteCard = (cardId) => {
    const updatedSavedCards = savedCards.filter(card => card.id !== cardId);
    console.log(updatedSavedCards)
    setSavedCards(updatedSavedCards);
    localStorage.setItem("savedCards", JSON.stringify(updatedSavedCards));
  };

  return (
    <div
      onDrop={handleDrop}
      onDragOver={allowDrop}
      className="saved-cards-container"
      style={{ minHeight: "100px", border: "2px dashed #cce", padding: "10px" }}
    >
      <h2 style={{ color: "white" }}>Saved Cards</h2>
      {savedCards.map((card) => (
        <div key={card.id} style={{ position: "relative" }}>
          {/* Delete button in the top-left corner */}
          <button
            onClick={() => handleDeleteCard(card.id)}
            style={{
              position: "absolute",
              top: "5px",
              left: "5px",
              backgroundColor: "red",
              color: "white",
              border: "none",
              borderRadius: "50%",
              cursor: "pointer",
              width: "20px",
              height: "20px",
              display: "flex",
              alignItems: "center",
              justifyContent: "center"
            }}
          >
            &times;
          </button>

          <TransactionCard
            group={card}
            activeCardIds={[]} // Empty array for saved cards
            newGroupIds={[]} // No new group background for saved cards
            highlightedGroupId={null} // No highlight for saved cards
            showUpdatedText={{}} // No updated text for saved cards
            handleCardClick={() => { }} // No action for click in saved area
            copyToClipboard={() => { }} // Copy function can be customized if needed
            savedCard={true}
            cexName={card.cexName}
          />
        </div>
      ))}
    </div>
  );
};

export default SavedCards;

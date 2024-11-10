const TransactionCard = ({ group, activeCardIds, newGroupIds, highlightedGroupId, showUpdatedText, handleCardClick, copyToClipboard, savedCard, cexName }) => (
  <div
    className={`transaction-card ${activeCardIds.includes(group.id) ? "active" : ""}`}
    onClick={() => handleCardClick(group.id)}
    draggable={!savedCard} // Only draggable if it's not a saved card
    onDragStart={(event) => {
      if (!savedCard) {
        event.dataTransfer.setData("cardData", JSON.stringify(group));
      }
    }}
    style={{
      backgroundColor: newGroupIds.includes(group.id)
        ? 'rgba(0, 200, 0, 0.1)'
        : highlightedGroupId === group.id
          ? 'rgba(200, 200, 0, 0.1)'
          : ''
    }}
  >
    <div
      className="copy-icon"
      onClick={(e) => {
        e.stopPropagation();
        copyToClipboard(group);
      }}
      style={{ position: "absolute", cursor: 'pointer', color: "white", transform: "translateX(980%)" }}
    >
      <span className="material-symbols-outlined">content_copy</span>
    </div>

    {/* Display CEX Name only if savedCard is true */}
    {savedCard && <h2 style={{ color: 'white' }}>{cexName || "Unknown CEX"}</h2>}

    <h3 style={{ color: 'white' }}>{new Date(group.created).toLocaleTimeString()}</h3>

    {showUpdatedText[group.id] && !savedCard && (
      <p style={{ color: 'yellow', margin: 0 }}>*Updated at {new Date(group.updatedAt).toLocaleTimeString()}*</p>
    )}

    <p><strong>Created:</strong> {new Date(group.created).toLocaleDateString()}</p>
    <p><strong>Time different:</strong> {group.timeDifferentUnix} seconds</p>
    <hr />
    <div className="transactions-headers">
      <h4>Address</h4>
      <h4>SOL</h4>
      {/* Conditionally render "Active" column based on savedCard */}
      {!savedCard && <h4>Active</h4>}
    </div>

    {group.transactions.map((transaction) => (
      <div key={transaction.id} className="transaction-detail">
        <a
          href={`https://solscan.io/account/${transaction.address}#transfers`}
          target="_blank"
          rel="noopener noreferrer"
          className="transaction-address-link"
          onClick={(e) => e.stopPropagation()}
        >
          {transaction.address.slice(0, 4)}...{transaction.address.slice(-4)}
        </a>
        <p>{transaction.sol}</p>

        {/* Conditionally render solChanged status based on savedCard */}
        {!savedCard && (
          <span className={`status-icon ${transaction.solChanged ? "red" : "green"}`}>
            {transaction.solChanged ? (
              <i className="fas fa-times"></i>
            ) : (
              <i className="fas fa-check"></i>
            )}
          </span>
        )}
      </div>
    ))}
  </div>
);

export default TransactionCard;

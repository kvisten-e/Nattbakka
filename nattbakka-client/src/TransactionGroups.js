import React, { useEffect, useState, useMemo } from "react";
import * as signalR from "@microsoft/signalr";
import "./TransactionGroup.css"; // Import CSS for styling
import "@fortawesome/fontawesome-free/css/all.min.css";

const TransactionList = () => {
  const [transactionGroups, setTransactionGroups] = useState([]);
  const [cex, setCex] = useState([]);

  const [activeCardIds, setActiveCardIds] = useState(
    JSON.parse(localStorage.getItem("activeCardIds")) || []
  );
  const [updatedGroup, setUpdatedGroup] = useState(null);
  const [highlightedGroupId, setHighlightedGroupId] = useState(null);
  const [showUpdatedText, setShowUpdatedText] = useState({});
  const [newGroupIds, setNewGroupIds] = useState([]);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [selectedCexId, setSelectedCexId] = useState(null);

  useEffect(() => {
    if (cex.length > 0 && selectedCexId === null) {
      setSelectedCexId(cex[0].id); 
    }
  }, [cex, selectedCexId]);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768); // Update mobile state based on window width
    };

    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("https://localhost:7066/cexGroupHub")
      .withAutomaticReconnect()
      .build();

    const fetchInitialTransactionGroups = async () => {
      try {
        const response = await fetch("https://localhost:7066/api/TransactionGroup");
        const data = await response.json();
        setTransactionGroups(data);
      } catch (error) {
        console.error("Error fetching initial transaction groups:", error);
      }
    };

    const fetchInitialCexObjects = async () => {
      try {
        const response = await fetch("https://localhost:7066/api/Cex");
        const data = await response.json();
        setCex(data);
      } catch (error) {
        console.error("Error fetching initial transaction groups:", error);
      }
    };

    connection.on("ReceiveTransactionGroupUpdate", (updatedGroup) => {
      setTransactionGroups((prevGroups) => {
        const index = prevGroups.findIndex(g => g.id === updatedGroup.id);
        const currentTime = new Date();
        updatedGroup.updatedAt = currentTime;

        if (index > -1) {
          const updatedGroups = [...prevGroups];
          updatedGroups[index] = updatedGroup;
          return updatedGroups;
        } else {
          // New group logic
          setNewGroupIds((prevIds) => [...prevIds, updatedGroup.id]);
          setTimeout(() => {
            setNewGroupIds((prevIds) => prevIds.filter(id => id !== updatedGroup.id));
          }, 20000); // Remove the new group ID after 20 seconds
          return [...prevGroups, updatedGroup];
        }
      });
    });

    connection.on("ReceiveNewTransactionToGroup", (transaction) => {
      setTransactionGroups((prevGroups) => {
        const groupIndex = prevGroups.findIndex(group => group.id === transaction.groupId);

        if (groupIndex !== -1) {
          const updatedGroups = [...prevGroups];
          const updatedGroup = { ...updatedGroups[groupIndex] };

          updatedGroup.transactions = [...updatedGroup.transactions, transaction];
          const currentTime = new Date();
          updatedGroup.updatedAt = currentTime;
          updatedGroups[groupIndex] = updatedGroup;

          setUpdatedGroup(updatedGroup);
          setHighlightedGroupId(updatedGroup.id);

          return updatedGroups;
        }

        return prevGroups;
      });
    }); 

    connection.start()
      .then(() => console.log("SignalR Connected"))
      .catch((err) => console.log("Connection failed: ", err));

    fetchInitialTransactionGroups();
    fetchInitialCexObjects();

    return () => {
      connection.stop();
    };
  }, []);

  useEffect(() => {
    if (highlightedGroupId) {
      const timer = setTimeout(() => {
        setHighlightedGroupId(null); 
        setShowUpdatedText((prev) => ({ ...prev, [highlightedGroupId]: true }));
      }, 20000);

      return () => clearTimeout(timer); 
    }
  }, [highlightedGroupId]);

  const handleCardClick = (id) => {
    setActiveCardIds((prevIds) => {
      let updatedIds;
      if (prevIds.includes(id)) {
        updatedIds = prevIds.filter((cardId) => cardId !== id);
      } else {
        updatedIds = [...prevIds, id];
      }
      localStorage.setItem("activeCardIds", JSON.stringify(updatedIds));
      return updatedIds;
    });
  };

  const cexIdToNameMap = useMemo(() => {
    return cex.reduce((acc, item) => {
      acc[item.id] = item.name;
      return acc;
    }, {});
  }, [cex]);

  const groupedByCexId = useMemo(() => {
    return transactionGroups.reduce((acc, group) => {
      group.transactions.forEach((transaction) => {
        const { cexId } = transaction;
        if (!acc[cexId]) acc[cexId] = [];
        if (!acc[cexId].some(g => g.id === group.id)) {
          acc[cexId].push(group);
        }
      });
      return acc;
    }, {});
  }, [transactionGroups]);

  const copyToClipboard = (group) => {
    const transactionLinks = group.transactions.map(transaction =>
      `https://solscan.io/account/${transaction.address}#transfers`
    ).join('\n');

    const uniqueSolValues = [...new Set(group.transactions.map(transaction => transaction.sol))].join(", ");
    const textToCopy = `${group.transactions.length}st - ${cexIdToNameMap[group.transactions[0].cexId] || "Unknown"} - ${uniqueSolValues}\n${transactionLinks}`;

    // Check if navigator.clipboard.writeText is available
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(textToCopy)
        .then(() => {
          console.log("Text copied to clipboard!");
        })
        .catch((err) => console.error("Could not copy text: ", err));
    } else {
      // Fallback for older browsers
      const textArea = document.createElement("textarea");
      textArea.value = textToCopy;
      textArea.style.position = "fixed"; // Prevent scrolling to bottom
      textArea.style.opacity = "0"; // Hide it visually
      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();

      try {
        const successful = document.execCommand("copy");
        if (successful) {
          console.log("Text copied to clipboard!");
        } else {
          console.error("Fallback: Could not copy text");
        }
      } catch (err) {
        console.error("Fallback: Could not copy text", err);
      }

      document.body.removeChild(textArea);
    }
  };

  return (
    <div className="transaction-container">
      {isMobile ? (
        <div className="dropdown-container">
          <label htmlFor="cex-dropdown">Select CEX:</label>
          <select
            id="cex-dropdown"
            value={selectedCexId || ''}
            onChange={(e) => setSelectedCexId(e.target.value)}
          >
            <option value="" disabled>Empty</option>
            {Object.keys(groupedByCexId).map(cexId => (
              <option key={cexId} value={cexId}>
                {cexIdToNameMap[cexId] || `CEX ID: ${cexId}`}
              </option>
            ))}
          </select>
        </div>
      ) : null}

      <div className={isMobile ? "transaction-cards" : "transaction-columns"}>
        {isMobile
          ? selectedCexId &&
          groupedByCexId[selectedCexId]
            .slice()
            .reverse()
            ?.map((group) => (
            <TransactionCard
              key={group.id}
              group={group}
              activeCardIds={activeCardIds}
              newGroupIds={newGroupIds}
              highlightedGroupId={highlightedGroupId}
              showUpdatedText={showUpdatedText}
              handleCardClick={handleCardClick}
              copyToClipboard={copyToClipboard}
            />
          ))
          : Object.keys(groupedByCexId).map((cexId) => (
            <div key={cexId} className="transaction-column">
              <h2>{cexIdToNameMap[cexId] || `CEX ID: ${cexId}`}</h2>
              {groupedByCexId[cexId]
                .slice()
                .reverse()
                .map((group) => (
                <TransactionCard
                  key={group.id}
                  group={group}
                  activeCardIds={activeCardIds}
                  newGroupIds={newGroupIds}
                  highlightedGroupId={highlightedGroupId}
                  showUpdatedText={showUpdatedText}
                  handleCardClick={handleCardClick}
                  copyToClipboard={copyToClipboard}
                />
              ))}
            </div>
          ))}
      </div>
    </div>
  );
};

// TransactionCard component for individual card rendering
const TransactionCard = ({ group, activeCardIds, newGroupIds, highlightedGroupId, showUpdatedText, handleCardClick, copyToClipboard }) => (
  <div
    className={`transaction-card ${activeCardIds.includes(group.id) ? "active" : ""}`}
    onClick={() => handleCardClick(group.id)}
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
    <h3>{new Date(group.created).toLocaleTimeString()}</h3>
    {showUpdatedText[group.id] && (
      <p style={{ color: 'yellow', margin: 0 }}>*Updated at {new Date(group.updatedAt).toLocaleTimeString()}*</p>
    )}
    <p><strong>Created:</strong> {new Date(group.created).toLocaleDateString()}</p>
    <p><strong>Time different:</strong> {group.timeDifferentUnix} seconds</p>
    <hr />
    <div className="transactions-headers">
      <h4>Address</h4>
      <h4>SOL</h4>
      <h4>Active</h4>
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
        <span className={`status-icon ${transaction.solChanged ? "red" : "green"}`}>
          {transaction.solChanged ? (
            <i className="fas fa-times"></i>
          ) : (
            <i className="fas fa-check"></i>
          )}
        </span>
      </div>
    ))}
  </div>
);

export default TransactionList;
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
        if (index > -1) {
          const updatedGroups = [...prevGroups];

          const currentTime = new Date();
          updatedGroup.updatedAt = currentTime;
          
          updatedGroups[index] = updatedGroup;
          return updatedGroups;
        } else {
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

  return (
    <div className="transaction-columns-container">
      <div className="transaction-columns">
        {Object.keys(groupedByCexId).map((cexId) => (
          <div key={cexId} className="transaction-column">
            <h2>{cexIdToNameMap[cexId] || `CEX ID: ${cexId}`}</h2>
            {groupedByCexId[cexId]
              .slice()
              .reverse()
              .map((group) => (
                <div
                  key={group.id}
                  className={`transaction-card ${activeCardIds.includes(group.id) ? "active" : ""}`}
                  onClick={() => handleCardClick(group.id)}
                  style={{
                    backgroundColor: highlightedGroupId === group.id
                      ? 'rgba(255, 255, 0, 0.2)'
                      : ''
                  }}                 
                >
                  <h3>{new Date(group.created).toLocaleTimeString()}</h3>
                  {showUpdatedText[group.id] && (
                    <p style={{ color: 'yellow', margin: 0 }}>*Updated at {new Date(group.updatedAt).toLocaleTimeString()}*</p>
                  )}                   
                  <p><strong>Created:</strong> {new Date(group.created).toLocaleDateString()}</p>
                  <p><strong>Time different:</strong> {group.timeDifferentUnix} seconds</p>
                  <hr />
                  <div class="transactions-headers">
                    <h4>Address</h4>
                    <h4>SOL</h4>
                    <h4>Active</h4>
                  </div>
                  {group.transactions
                    .filter((transaction) => transaction.cexId === parseInt(cexId))
                    .map((transaction) => (
                      <div key={transaction.id} className="transaction-detail">
                        <a
                          href={`https://solscan.io/account/${transaction.address}#transfers`}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="transaction-address-link"
                        >
                          {transaction.address.slice(0, 4)}...{transaction.address.slice(-4)}
                        </a>
                        <p>{transaction.sol}</p>
                        <span className={`status-icon ${transaction.solChanged ? "red" : "green"}`}>
                          {transaction.solChanged ? (
                            <i className="fas fa-times"></i> // Red cross
                          ) : (
                            <i className="fas fa-check"></i> // Green checkmark
                          )}
                        </span>
                      </div>
                    ))}
                </div>
              ))}
          </div>
        ))}
      </div>
    </div>
  );
};

export default TransactionList;

import React, { useEffect, useState, useMemo } from "react";
import * as signalR from "@microsoft/signalr";
import "./TransactionGroup.css"; // Import CSS for styling

const TransactionList = () => {
  const [transactionGroups, setTransactionGroups] = useState([]);

  useEffect(() => {
    // Initialize SignalR connection
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5000/transactionHub") // Update to your server URL
      .withAutomaticReconnect()
      .build();

    // Fetch initial transaction groups
    const fetchInitialTransactionGroups = async () => {
      try {
        const response = await fetch("https://localhost:7066/api/TransactionGroup");
        const data = await response.json();
        setTransactionGroups(data);
      } catch (error) {
        console.error("Error fetching initial transaction groups:", error);
      }
    };

    // Handle updates from the server
    connection.on("ReceiveTransactionGroupUpdate", (updatedGroup) => {
      setTransactionGroups((prevGroups) => {
        const index = prevGroups.findIndex(g => g.id === updatedGroup.id);
        if (index > -1) {
          // Update existing group
          const updatedGroups = [...prevGroups];
          updatedGroups[index] = updatedGroup;
          return updatedGroups;
        } else {
          // Add new group if it doesn't already exist
          return [...prevGroups, updatedGroup];
        }
      });
    });

    connection.start()
      .then(() => console.log("SignalR Connected"))
      .catch((err) => console.log("Connection failed: ", err));

    fetchInitialTransactionGroups();

    return () => {
      connection.stop();
    };
  }, []);

  // Memoize the grouped data by `cexId` to avoid recalculating it on every render
  const groupedByCexId = useMemo(() => {
    return transactionGroups.reduce((acc, group) => {
      group.transactions.forEach((transaction) => {
        const { cexId } = transaction;
        if (!acc[cexId]) acc[cexId] = [];
        // Ensure that we don't add duplicate groups for a specific `cexId`
        if (!acc[cexId].some(g => g.id === group.id)) {
          acc[cexId].push(group);
        }
      });
      return acc;
    }, {});
  }, [transactionGroups]);

  return (
    <div className="transaction-columns">
      {Object.keys(groupedByCexId).map((cexId) => (
        <div key={cexId} className="transaction-column">
          <h3>CEX ID: {cexId}</h3>
          {groupedByCexId[cexId].map((group) => (
            <div key={group.id} className="transaction-card">
              <p>Created: {new Date(group.created).toLocaleString()}</p>
              <p>Time different: {group.timeDifferentUnix} seconds</p>
              <hr />
              {group.transactions
                .filter((transaction) => transaction.cexId === parseInt(cexId))
                .map((transaction) => (
                  <div key={transaction.id} className="transaction-detail">
                    <p>Address: {transaction.address.slice(0, 4)}...{transaction.address.slice(-4)}</p>
                    <p>SOL: {transaction.sol}</p>
                  </div>
                ))}
            </div>
          ))}
        </div>
      ))}
    </div>
  );
};

export default TransactionList;

# Yummiez Project Updates 2: Order & Driver Tracking Feature

This document explicitly details the new Order Placement and Live Driver Tracking feature added to the Yummiez application.

## 🎯 Feature Overview
We implemented an interactive order flow designed to mimic platforms like DoorDash. Users can now place an order at any open restaurant, and immediately track their delivery driver approaching their location via a live, animated map.

## 🛠️ Technical Implementation

### 1. Database & Models (`Order.cs`)
- **New Entity**: Created an `Order` model representing the order lifecycle.
- **Tracking Fields**: Included specific geographical coordinate columns (`RestaurantLat`, `RestaurantLng`, `DestLat`, `DestLng`, `DriverLat`, `DriverLng`) to manage spatial tracking entirely within our backend without requiring paid routing APIs.
- **Safe Azure DB Migration**: Modified the EF Core migration script using advanced SQL checks. This prevents deployment crashes by securely dropping previous schema artifacts (like foreign key constraint `FK_Orders_Restaurants_restaurant_id`) if the database was already provisioned in Azure.

### 2. Live Tracking Backend (`OrderTrackingController.cs`)
- **Simulation Engine**: Built a dedicated API endpoint (`/api/orders/{id}/track`).
- **Progressive Updates**: When the frontend polls this endpoint every 2 seconds, the backend seamlessly calculates the driver's progress (interpolating coordinates directly between the restaurant and the destination) and increments a step counter.
- **Status Workflow**: The server programmatically transitions the order status from `Placed` → `Preparing` → `Picked Up` → `On the Way` → `Delivered`.

### 3. Frontend & Map Integration (`Track.cshtml` & `Details.cshtml`)
- **Order Placement Entry**: Updated the Restaurant Details page (`Details.cshtml`) with a secure "Place an Order" form dynamically displayed only for authenticated users browsing an "Open" restaurant.
- **Leaflet.js Mapping**: The new tracking page (`Track.cshtml`) embeds an interactive map using the open-source **Leaflet.js** and **OpenStreetMap** layers.
  - Generates custom, stylized markers (🏪 Restaurant, 📍 Destination, 🚗 Moving Driver).
  - Draws a dotted line path representing the expected delivery route.
- **Dynamic Progress Bar**: Built an animated, 5-step status progression bar at the top of the tracking screen that visually syncs with the data returned from the backend.
- **My Orders Dashboard**: Added an `Index.cshtml` page displaying a user's chronological order history and its status badges, linked securely from the updated navigation bar layout.

## ✨ Presentation Highlights
For the project demonstration, this feature acts as the "Wow Factor" because:
*   It operates as a **Full-Stack mechanism** (backend physics + frontend visualizations).
*   The map and animation function immediately "out of the box" using free open-source tile layers—no fragile Google Maps API keys are necessary.
*   Simulated routes take exactly ~20 seconds to complete, matching the fast pace of a school presentation.

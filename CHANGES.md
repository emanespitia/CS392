# Yummiez — Changelog

## March 29, 2026

### 🔧 Bug Fixes

#### Role-Based Authorization
- **Fixed: New users not receiving a role on registration** — Added `AddToRoleAsync(user, "User")` in `Register.cshtml.cs` so newly registered accounts automatically get the "User" role and can access authorized pages.
- **Fixed: Homepage inaccessible to guests** — Uncommented `[AllowAnonymous]` on `Index.cshtml.cs` so unauthenticated visitors can browse the homepage without being redirected to login.
- **Fixed: Admin buttons visible to non-admins** — Wrapped "Add Restaurant", "Edit", and "Delete" buttons in `@if (User.IsInRole("Admin"))` on `Restaurants/Index.cshtml` so only admins see management actions.

#### Create/Edit/Delete Forms
- **Fixed: Internal fields exposed in forms** — Removed `AdminId`, `CreatedAt`, and `UpdatedAt` from Create and Edit forms. These are now auto-set server-side (`CreatedAt` on create, `UpdatedAt` on edit).
- **Fixed: IsOpen rendered as text input** — Changed to a toggle switch. Fixed a `bool?` crash with the `asp-for` tag helper by using manual HTML checkbox binding.
- **Fixed: Missing Category and ImageUrl fields** — Added a category dropdown (Burgers, Pizza, Sushi, Healthy, Desserts, Coffee, Mexican) and an Image URL input to both Create and Edit forms.

#### Cleanup
- **Deleted unused `_LoginPartial.cshtml`** — Was not referenced anywhere in the layout.
- **Fixed footer CSS** — Replaced old absolute-positioned footer with a proper static footer layout.
- **Removed `body { margin-bottom: 60px }` hack** — No longer needed with the new footer.

---

### ✨ New Features

#### Order Placement & Live Tracking
- **Order Flow** — Added an "Order Now" section to restaurant details pages, allowing users to enter a delivery address.
- **Simulated Driver Backend** — Created a tracking API (`/api/orders/{id}/track`) that simulates a driver navigating from the restaurant to the user's destination, advancing ~10% every poll.
- **Live Map UI** — Implemented `Orders/Track.cshtml` using Leaflet.js and OpenStreetMap (free, no API key). Shows a real map with custom markers for the restaurant 🏪, driver 🚗, and destination 📍.
- **Animated Progress** — Status bar updates in real-time as the driver simulates delivery (Placed → Preparing → Picked Up → On the Way → Delivered).
- **Order History** — Added a "My Orders" page (`/Orders/Index`) that lists past orders with interactive status badges and dates.

#### Search Functionality
- Wired up the homepage search bar to filter restaurants by **name** or **address**.
- Search uses a GET form so results are shareable via URL.

#### About Page (`/About`)
- New page showcasing the project mission, technology stack (ASP.NET Core 9.0, Azure SQL, Bootstrap 5, EF Core, Git), and a feature checklist.
- Accessible to all users (guests included).

#### Privacy Page (`/Privacy`)
- Replaced the default placeholder with real privacy policy content covering data collection, security measures, and academic use disclaimer.

#### Admin Dashboard Stats
- Added 4 stat cards at the top of the Admin Dashboard:
  - **Total Restaurants** — count of all restaurants in the database
  - **Open Now** — restaurants currently marked as open
  - **Closed** — restaurants currently closed
  - **Registered Users** — total user accounts

#### Toast Notifications
- Success/error alert banners now appear after:
  - Creating a restaurant
  - Editing a restaurant
  - Deleting a restaurant
  - Promoting a user to Admin
  - Demoting an Admin to User
  - Attempting to demote the last admin (error toast)

#### Enhanced Footer
- Replaced the minimal one-line footer with a 3-column layout:
  - **Branding** — Yummiez logo and tagline
  - **Quick Links** — Home, Restaurants, About, Privacy
  - **Built With** — ASP.NET Core 9.0, Azure SQL Server, Bootstrap 5

#### Navbar Updates
- Added **About** link to the main navigation bar.

#### Form Styling
- Create, Edit, and Delete pages fully restyled with:
  - Floating label inputs
  - Green focus states matching the app theme
  - Card-based layout with header icons
  - Consistent button styling

---

### 📁 Files Changed

| File | Change |
|---|---|
| `Register.cshtml.cs` | Role assignment on registration |
| `Index.cshtml.cs` | `[AllowAnonymous]` + search logic |
| `Index.cshtml` | Search form wiring |
| `Restaurants/Index.cshtml` | Admin-only button guards |
| `Restaurants/Create.cshtml` | Full form redesign |
| `Restaurants/Create.cshtml.cs` | Auto-set fields + toast |
| `Restaurants/Edit.cshtml` | Full form redesign |
| `Restaurants/Edit.cshtml.cs` | Auto-set UpdatedAt + toast |
| `Restaurants/Delete.cshtml` | Redesigned confirmation page |
| `Restaurants/Delete.cshtml.cs` | Toast notification |
| `Admin/Index.cshtml` | Stats cards + table redesign |
| `Admin/Index.cshtml.cs` | Stats queries + toasts |
| `About.cshtml` + `.cs` | **NEW** |
| `Privacy.cshtml` | Real content |
| `Shared/_Layout.cshtml` | Navbar, footer, toast container |
| `wwwroot/css/site.css` | Footer + form styles |
| `_LoginPartial.cshtml` | **DELETED** |

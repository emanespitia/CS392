# Latest Changes — Yummiez Milestone Fixes
**Date:** March 3, 2026

---

## Phase 1: Role-Based Authorization (Milestones 3 & 4)

### Global Fallback Authorization Policy
**File:** `Program.cs`
- Added `using Microsoft.AspNetCore.Authorization;`
- Added a **fallback authorization policy** that requires all users to be authenticated by default
- Any page without `[AllowAnonymous]` now redirects unauthenticated users to the Login page
- Added `app.UseStatusCodePagesWithReExecute("/Error/{0}")` for proper error page routing

```csharp
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

### Restaurants CRUD — Authorization Added

| File | Attribute Added | Who Can Access |
|---|---|---|
| `Pages/Restaurants/Index.cshtml.cs` | `[Authorize]` | Any logged-in user |
| `Pages/Restaurants/Details.cshtml.cs` | `[Authorize]` | Any logged-in user |
| `Pages/Restaurants/Create.cshtml.cs` | `[Authorize(Roles = "Admin")]` | Admin only |
| `Pages/Restaurants/Edit.cshtml.cs` | `[Authorize(Roles = "Admin")]` | Admin only |
| `Pages/Restaurants/Delete.cshtml.cs` | `[Authorize(Roles = "Admin")]` | Admin only |

### Pages Kept Public
| File | Attribute Added | Reason |
|---|---|---|
| `Pages/Index.cshtml.cs` | `[AllowAnonymous]` (already existed) | Homepage stays public |
| `Pages/Privacy.cshtml.cs` | `[AllowAnonymous]` (newly added) | Privacy page stays public |

---

## Phase 2: Logging (Milestone 5)

### Failed Login Logging
**File:** `Areas/Identity/Pages/Account/Login.cshtml.cs`
- Added `_logger.LogWarning("Failed login attempt for email: {Email}", Input.Email)` when login fails
- This was the only login outcome not being logged (success and lockout were already logged)

### Restaurant CRUD Logging
**Files:** `Pages/Restaurants/Create.cshtml.cs`, `Edit.cshtml.cs`, `Delete.cshtml.cs`
- Injected `ILogger<T>` into each page model constructor
- Added `LogInformation` after each successful operation

```
Restaurant created: {Name} by {User}
Restaurant edited: ID={Id} by {User}
Restaurant deleted: ID={Id}, Name={Name} by {User}
```

### TestRecords CRUD Logging
**Files:** `Pages/TestRecords/Create.cshtml.cs`, `Edit.cshtml.cs`, `Delete.cshtml.cs`
- Injected `ILogger<T>` into each page model constructor
- Added `LogInformation` after each successful operation

```
TestRecord created: {Name} by {User}
TestRecord edited: ID={Id} by {User}
TestRecord deleted: ID={Id}, Name={Name} by {User}
```

---

## Phase 3: Custom Error Page (Milestone 5)

### Error Page Redesign
**Files:** `Pages/Error.cshtml` + `Pages/Error.cshtml.cs`

**Before:** Default ASP.NET error template with generic "An error occurred" message.

**After:**
- Accepts a `statusCode` route parameter (e.g., `/Error/404`)
- Displays **user-friendly messages** per status code:
  - `404` → "Page Not Found"
  - `403` → "Access Denied"
  - `401` → "Unauthorized"
  - `500` → "Server Error"
- Styled with **Yummiez green theme** (`#2e7d32`)
- Large status code number display
- **"Go Home" button** to return to homepage
- **Logs every error** with status code, request ID, and request path
- Marked `[AllowAnonymous]` so errors display even for unauthenticated users

---

## All Files Modified (13 total)

| # | File | What Changed |
|---|---|---|
| 1 | `Program.cs` | Global auth policy + status code pages middleware |
| 2 | `Pages/Restaurants/Index.cshtml.cs` | Added `[Authorize]` |
| 3 | `Pages/Restaurants/Details.cshtml.cs` | Added `[Authorize]` |
| 4 | `Pages/Restaurants/Create.cshtml.cs` | Added `[Authorize(Roles="Admin")]` + ILogger + logging |
| 5 | `Pages/Restaurants/Edit.cshtml.cs` | Added `[Authorize(Roles="Admin")]` + ILogger + logging |
| 6 | `Pages/Restaurants/Delete.cshtml.cs` | Added `[Authorize(Roles="Admin")]` + ILogger + logging |
| 7 | `Pages/TestRecords/Create.cshtml.cs` | Added ILogger + create logging |
| 8 | `Pages/TestRecords/Edit.cshtml.cs` | Added ILogger + edit logging |
| 9 | `Pages/TestRecords/Delete.cshtml.cs` | Added ILogger + delete logging |
| 10 | `Areas/Identity/Pages/Account/Login.cshtml.cs` | Added failed login warning log |
| 11 | `Pages/Privacy.cshtml.cs` | Added `[AllowAnonymous]` |
| 12 | `Pages/Error.cshtml` | Custom themed error page UI |
| 13 | `Pages/Error.cshtml.cs` | Status code handling + error logging |

---

## Build Status
```
Build succeeded — 0 errors, 0 new warnings
```

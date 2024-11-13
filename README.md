# Coupon Codes Management System

## Overview

This project is a backend system developed in C# with .NET, implementing a coupon management API for business administrators. The API allows the admin to create, update, delete, and manage coupons that customers can use for discounts. Customers can input coupon codes to receive discounts on a base order value of 100 ₪. The system also includes reporting capabilities, allowing admins to track coupon usage over specific time periods or by the creator.

## Features

### User Management
- **Admin User Management**: Admins have username and password authentication.
- **Registration and Login**: Only admins can register new users and manage their login sessions.
  
### Coupon Management
- **CRUD Operations**: Admins can create, read, update, and delete coupons.
- **Coupon Attributes**:
  - **Code**: A unique identifier for coupon use, only visible to admins.
  - **Description**: Only visible to admins.
  - **Discount**: Either a percentage or a fixed amount.
  - **Expiration Date**: Optional expiration for each coupon.
  - **Stackable Discounts**: Determines if the coupon can be combined with others.
  - **Usage Limit**: Limit on the number of times a coupon can be used.
- **Coupon Usage**: Customers can input multiple coupon codes for a discount on a predefined order value.

### Reporting System
- **View Coupon List**: Allows viewing of all coupons created by a specific user or within a date range.
- **Export to Excel**: Generates reports in Excel format for better data analysis.

## Technical Details

### Stack
- **Backend**: .NET, C#.
- **Database**: MySQL with Entity Framework for ORM.

### Validation and Security
- Model validations to ensure data consistency.
- API follows standard REST conventions, returning appropriate status codes and JSON-formatted responses.

### Requirements and Setup

To run this project locally:
1. Clone this repository.
2. Set up a MySQL database and update the connection string in the configuration.
3. Run the migrations to initialize the database structure.
4. Start the application to access the coupon management endpoints.

## Notes
- This project contains backend code only, focusing on the logic and API controllers. Frontend views and integrations were omitted due to time constraints.
- Some frontend code comments are included, which were part of initial testing and may be used for potential future expansions.

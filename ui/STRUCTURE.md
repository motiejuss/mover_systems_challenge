# UI Project Structure

This document describes the improved project structure implemented for the delivery routes application.

## Overview

The project has been restructured following feature-based architecture and modern React best practices, providing better organization, maintainability, and scalability.

## Directory Structure

```
ui/src/
├── App.tsx                          # Main application component
├── main.tsx                         # Application entry point
├── features/                        # Feature-based modules
│   └── delivery-routes/            # Delivery routes feature
│       ├── components/             # Feature-specific components
│       │   ├── AlgorithmSelector/
│       │   ├── VehicleEmissionSelector/
│       │   ├── OriginSelector/
│       │   ├── DestinationList/
│       │   ├── RouteControlPanel/
│       │   ├── RouteMap/
│       │   └── RouteResults/
│       ├── hooks/                  # Feature-specific custom hooks
│       │   ├── useGoogleMapsAutocomplete.ts
│       │   ├── usePlaceSelection.ts
│       │   └── useRouteCalculation.ts
│       ├── pages/                  # Feature pages
│       │   └── DeliveryRoutesPage.tsx
│       ├── types/                  # Feature-specific types
│       │   └── index.ts
│       └── utils/                  # Feature-specific utilities
│           └── route.utils.ts
└── shared/                         # Shared resources
    ├── config/                     # Configuration files
    │   ├── api.config.ts
    │   └── maps.config.ts
    └── services/                   # Shared services
        └── api.service.ts
```

## Key Improvements

### 1. **Feature-Based Organization**

- Code is organized by feature rather than by type
- Each feature is self-contained with its own components, hooks, types, and utilities
- Easier to locate related code and understand feature boundaries

### 2. **Separation of Concerns**

- **Components**: UI presentation separated into focused, reusable components
- **Hooks**: Business logic extracted into custom hooks
- **Services**: API calls centralized in service layer
- **Config**: Environment and configuration settings isolated
- **Types**: TypeScript types properly organized and exported
- **Utils**: Helper functions separated from components

### 3. **Component Structure**

Each component follows the pattern:

```
ComponentName/
├── ComponentName.tsx    # Component implementation
└── index.ts            # Clean export
```

### 4. **Custom Hooks**

- `useGoogleMapsAutocomplete`: Manages Google Maps autocomplete initialization
- `usePlaceSelection`: Handles origin and destination selection state
- `useRouteCalculation`: Manages route calculation logic and API calls

### 5. **Shared Resources**

- Configuration files for API and Google Maps settings
- Centralized API service for route calculations
- Environment variable management via `.env` file

## Component Breakdown

### Control Panel Components

- **AlgorithmSelector**: Dropdown for selecting routing algorithm
- **VehicleEmissionSelector**: Dropdown for selecting vehicle type
- **OriginSelector**: Origin selection with Google Maps autocomplete
- **DestinationList**: Destination management with autocomplete
- **RouteControlPanel**: Main control panel container

### Map & Results Components

- **RouteMap**: Google Maps display with route visualization
- **RouteResults**: Route information and statistics display

### Page Components

- **DeliveryRoutesPage**: Main page that composes all components

## Type Safety

All components are fully typed with TypeScript:

- Interface definitions for props
- Type exports for shared types
- Proper typing for API responses
- Type-safe hook return values

## Configuration

### Environment Variables

Create a `.env` file in the `ui` directory:

```env
VITE_GOOGLE_MAPS_API_KEY=your_api_key_here
```

### API Configuration

API endpoints and settings in `shared/config/api.config.ts`

### Maps Configuration

Google Maps settings in `shared/config/maps.config.ts`

## Benefits

1. **Maintainability**: Clear structure makes code easier to understand and modify
2. **Scalability**: Easy to add new features or extend existing ones
3. **Reusability**: Shared components and hooks can be reused across features
4. **Type Safety**: Full TypeScript coverage prevents runtime errors
5. **Testability**: Isolated components and hooks are easier to test
6. **Developer Experience**: Logical organization speeds up development

## Migration Notes

The old structure with top-level `components/`, `pages/`, `services/`, `constants/`, and `types/` directories has been replaced with the feature-based structure described above.

All functionality has been preserved and improved with:

- Better type safety
- Cleaner component APIs
- Improved separation of concerns
- More maintainable code organization

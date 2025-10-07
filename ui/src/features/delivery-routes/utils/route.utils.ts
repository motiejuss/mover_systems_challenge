/**
 * Parse duration string to minutes
 * Handles formats like: "736s", "5 mins", "1 hour 30 mins", "45 secs"
 */
export const parseDurationToMinutes = (duration: string): number => {
  let totalMinutes = 0;

  // Match hours
  const hoursMatch = duration.match(/(\d+)\s*hour/i);
  if (hoursMatch) {
    totalMinutes += parseInt(hoursMatch[1]) * 60;
  }

  // Match minutes
  const minsMatch = duration.match(/(\d+)\s*min/i);
  if (minsMatch) {
    totalMinutes += parseInt(minsMatch[1]);
  }

  // Match seconds (convert to minutes) - handles both "45 secs" and "736s"
  const secsMatch = duration.match(/(\d+)\s*s(?:ec)?(?:s)?/i);
  if (secsMatch) {
    totalMinutes += Math.ceil(parseInt(secsMatch[1]) / 60);
  }

  return totalMinutes || 0;
};

/**
 * Format distance in meters to km with precision
 */
export const formatDistance = (
  meters: number,
  precision: number = 2
): string => {
  return `${(meters / 1000).toFixed(precision)} km`;
};

/**
 * Format distance in meters to km (short format)
 */
export const formatDistanceShort = (meters: number): string => {
  return `${(meters / 1000).toFixed(1)} km`;
};

/**
 * Generate color for route visualization
 */
export const getRouteColor = (order: number): string => {
  const colors = [
    "#3B82F6", // blue
    "#10B981", // green
    "#F59E0B", // amber
    "#EF4444", // red
    "#8B5CF6", // purple
    "#EC4899", // pink
  ];
  return colors[order % colors.length];
};

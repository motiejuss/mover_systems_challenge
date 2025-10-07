import { useEffect, useRef } from "react";
import type { SelectedPlace, PlaceSelectEvent } from "../types";

interface UseGoogleMapsAutocompleteProps {
  isLoaded: boolean;
  onPlaceSelect: (place: SelectedPlace) => void;
}

export const useGoogleMapsAutocomplete = ({
  isLoaded,
  onPlaceSelect,
}: UseGoogleMapsAutocompleteProps) => {
  const autocompleteRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!isLoaded || !autocompleteRef.current) return;

    const initAutocomplete = async () => {
      const placesLibrary = (await window.google.maps.importLibrary(
        "places"
      )) as google.maps.PlacesLibrary & {
        BasicPlaceAutocompleteElement: new () => HTMLElement & {
          addEventListener: (
            event: string,
            callback: (e: Event) => void
          ) => void;
        };
      };

      const autocompleteElement =
        new placesLibrary.BasicPlaceAutocompleteElement();

      autocompleteElement.addEventListener(
        "gmp-select",
        async (event: Event) => {
          const placeEvent = event as PlaceSelectEvent;
          const place = placeEvent.place;

          await place.fetchFields({
            fields: ["formattedAddress", "displayName", "location"],
          });

          const selectedPlace: SelectedPlace = {
            id: place.id,
            placeId: place.id,
            address: place.formattedAddress || place.displayName || "",
            lat: place.location?.lat(),
            lng: place.location?.lng(),
          };

          onPlaceSelect(selectedPlace);
        }
      );

      if (autocompleteRef.current) {
        autocompleteRef.current.innerHTML = "";
        autocompleteRef.current.appendChild(autocompleteElement);
      }
    };

    initAutocomplete();
  }, [isLoaded, onPlaceSelect]);

  return autocompleteRef;
};

using System.Collections.Generic;
using System.Linq;
using Game.Level.Unit;
using UnityEngine;

namespace Game.Level.Line
{
    public sealed class LineController
    {
        private readonly Dictionary<Transform, UnitController> _placeUnitMap;

        public Dictionary<Transform, UnitController> PlaceUnitMap => _placeUnitMap;

        public LineController(RouteView line)
        {
            _placeUnitMap = new Dictionary<Transform, UnitController>();

            foreach (var point in line.Points)
            {
                _placeUnitMap.Add(point, null);
            }
        }

        public Transform GetAvailablePlace()
        {
            foreach (var place in _placeUnitMap.Keys)
            {
                var unit = _placeUnitMap[place];
                if (unit == null)
                    return place;
            }
            return null;
        }

        public UnitController GetFirstCustomer()
        {
            var customer = _placeUnitMap.Values.First();
            return customer;
        }

        /// <summary>
        /// True if the front-of-line slot has a customer that has actually walked up
        /// (within ~0.1 units of the place transform). Used by ReceptionModule so the
        /// receptionist's progress timer doesn't tick down until a guest is physically
        /// at the desk — otherwise multi-worker rooms drain the line faster than
        /// customers can arrive, causing them to be teleported mid-walk and visually
        /// "skip" reception.
        /// </summary>
        public bool IsFirstCustomerReady()
        {
            var firstPlace = _placeUnitMap.Keys.First();
            var customer = _placeUnitMap[firstPlace];
            if (customer == null) return false;
            return (customer.View.transform.position - firstPlace.position).sqrMagnitude <= 0.04f;
        }

        /// <summary>
        /// FIX #3: Cache dictionary keys into a list for O(n) iteration.
        /// Previously used .ToList() in a foreach then .ElementAt(index) inside the loop.
        /// ElementAt() on a Dictionary is O(n) per call, making the whole method O(n³).
        /// Now keys are cached once, and direct index access gives O(n) total.
        /// </summary>
        public void RearrangeCustomersLine()
        {
            var places = _placeUnitMap.Keys.ToList();
            for (int i = 0; i < places.Count; i++)
            {
                var place = places[i];
                UnitController customer = null;

                int nextIndex = i + 1;
                if (nextIndex < places.Count)
                {
                    customer = _placeUnitMap[places[nextIndex]];
                }

                _placeUnitMap[place] = customer;

                if (customer != null)
                    customer.SwitchToState(new UnitWalkState(place.transform.position));
            }
        }
    }
}


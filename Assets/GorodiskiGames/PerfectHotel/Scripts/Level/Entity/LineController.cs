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

        public void RearrangeCustomersLine()
        {
            int index = 0;
            foreach (var point in _placeUnitMap.Keys.ToList())
            {
                var place = _placeUnitMap.ElementAt(index).Key;
                UnitController customer = null;

                int customerIndex = index + 1;
                if (customerIndex < _placeUnitMap.Count)
                {
                    customer = _placeUnitMap.ElementAt(customerIndex).Value;
                }

                _placeUnitMap[place] = customer;

                if (customer != null)
                    customer.SwitchToState(new UnitWalkState(place.transform.position));

                index++;
            }
        }
    }
}


using DynamicData.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalrClient.ViewModel
{
    public class FilterItem : AbstractNotifyPropertyChanged, IEquatable<FilterItem>
    {
        private FilterCategory _filterCategoryItem;
        private bool _isSelected;
        private bool _isSelectedOnOpen;
        private string _description;

        public enum FilterCategory
        {
            Moniker,
            Algo,
            Trader,
            Sender,
            Status,
            Symbol,
            Currency,
            Exchange,
            Urgency,
            Side,
            System,
            TgtAlgo,
            TgtSub
        }
        public FilterCategory FilterCategoryItem
        {
            get { return _filterCategoryItem; }
            set
            {
                _filterCategoryItem = value;
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetAndRaise( ref _isSelected , value);
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                }
            }
        }

        public void ValidateValue(HashSet<string> filtList)
        {
            _isSelectedOnOpen = IsSelected;
            if (IsSelected)
                filtList.Add(this.Description);
            else
                filtList.Remove(this.Description);
        }

        public FilterItem()
        {
            IsSelected = _isSelectedOnOpen = false;
        }

        public FilterItem(FilterCategory categoryItem, string desc)
        {
            FilterCategoryItem = categoryItem;
            this.Description = desc;
            IsSelected = _isSelectedOnOpen = false;
        }

        public FilterItem(FilterCategory categoryItem, string description, bool isSelected)
        {
            FilterCategoryItem = categoryItem;
            Description = description;
            IsSelected = _isSelectedOnOpen = isSelected;
        }

        public FilterItem(FilterCategory categoryItem, string description, bool isSelected, HashSet<string> filtList)
        {
            FilterCategoryItem = categoryItem;
            Description = description;
            IsSelected = _isSelectedOnOpen = isSelected;
            ValidateValue(filtList);
        }

        // Use this call on Cancel click
        public void RestoreOpenValue()
        {
            IsSelected = _isSelectedOnOpen;
        }

        public bool Equals(FilterItem other)
        {
            return other != null && string.Equals(other.Description, this.Description) && other.FilterCategoryItem == this.FilterCategoryItem;
        }

    }
}

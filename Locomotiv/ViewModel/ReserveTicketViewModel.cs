using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.ViewModel
{
    public class ReserveTicketViewModel : BaseViewModel
    {
        private readonly ITrainDAL _trainDAL;

        public ObservableCollection<Train> AvailableTrains { get; }

        private Train _selectedTrain;
        public Train SelectedTrain
        {
            get => _selectedTrain;
            set
            {
                _selectedTrain = value;
                OnPropertyChanged(nameof(SelectedTrain));
            }
        }

        public ReserveTicketViewModel(ITrainDAL trainDAL)
        {
            _trainDAL = trainDAL;

            AvailableTrains = new ObservableCollection<Train>(
                _trainDAL.GetAllAvailablePassengerTrains()
            );
        }
    }
}

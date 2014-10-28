angular.module('disc.admin')
    .controller('AdminConsoleCtrl', [
        '$scope',
        'DisciturBaseCtrl',
        '$injector',
        'AdminService',
        'AuthService',
        '$state',
        function (
            $scope,
            DisciturBaseCtrl,
            $injector,
            AdminService,
            AuthService,
            $state
            ) {
            // inherit Discitur Base Controller
            $injector.invoke(DisciturBaseCtrl, this, { $scope: $scope });

            //-------- private properties -------
            $scope._ctrl = 'AdminConsoleCtrl';
            //-------- private methods -------

            //--------- public properties ------
            $scope.labels = {
                adminMigrate3Layer2CqrsEs: $scope.getLabel('adminMigrate3Layer2CqrsEs'),
            };

            $scope.local = {
                user: AuthService.user
            }
            //-------- public methods -------
            $scope.actions = {
                migrate: function () {
                    AdminService.migrate().then(
                        //TODO: add success/error message
                        function() { },
                        function() { }
                        );
                }
            }
            //--------- Controller initialization ------
            $scope.$watch(function () {
                return $scope.local.user.isLogged;
            },
                function (isLogged) {
                    if (!isLogged)
                        $state.go('lessonSearch', {}, { inherit: false });
                }
            );


        }
    ]);
